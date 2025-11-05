"use client"
import React, { memo, useEffect, useMemo, useRef, useState } from 'react'
import { useRouter } from 'next/navigation'
import { api } from '../../lib/api'
import { storage } from '../../lib/auth'
import { createAlertsConnection } from '../../lib/signalr'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useVirtualizer } from '@tanstack/react-virtual'
import { toast as notify } from 'sonner'

type Config = { id: string, tempMax: number, humidityMax: number, updatedAt: string }
type AlertDto = { id: string, type: string, value: number, threshold: number, createdAt: string, status: string }
// Precomputed render-friendly shape
type AlertRow = AlertDto & { createdAtStr: string, valueStr: string, thresholdStr: string }

export default function Dashboard() {
  const router = useRouter()
  const qc = useQueryClient()
  // Removed local toast state; use global toaster
  const [statusFilter, setStatusFilter] = useState<string>('open')
  const [sortBy, setSortBy] = useState<'createdAt'|'value'>('createdAt')
  const [sortDir, setSortDir] = useState<'desc'|'asc'>('desc')
  const [incomingQueue, setIncomingQueue] = useState<AlertDto[]>([])
  const MAX_ROWS = 200
  const FLUSH_MS = 1000
  const MAX_BATCH_PER_FLUSH = 200

  useEffect(() => {
    if (!storage.getToken()) router.replace('/login')
  }, [router])

  const cfgQuery = useQuery<Config>({
    queryKey: ['config'],
    queryFn: api.getConfig,
  })

  const [tempMax, setTempMax] = useState<number>(80)
  const [humidityMax, setHumidityMax] = useState<number>(70)

  useEffect(() => {
    if (cfgQuery.data) {
      setTempMax(cfgQuery.data.tempMax)
      setHumidityMax(cfgQuery.data.humidityMax)
    }
  }, [cfgQuery.data])

  const alertsQuery = useQuery<AlertDto[]>({
    queryKey: ['alerts', statusFilter],
    queryFn: () => api.listAlerts(statusFilter || undefined),
    initialData: [],
  })

  // Periodically flush queued alerts into react-query cache to reduce re-renders
  useEffect(() => {
    if (incomingQueue.length === 0) return
    const id = setInterval(() => {
      setIncomingQueue((queue) => {
        if (queue.length === 0) return queue
        const batch = queue.slice(0, MAX_BATCH_PER_FLUSH)
        qc.setQueryData<AlertDto[]>(['alerts', statusFilter], (old) => {
          const existing = old || []
          // Build a set of existing ids to prevent duplicates
          const seen = new Set(existing.map(x => x.id))
          const uniqueBatch = batch.filter(x => !seen.has(x.id))
          const merged = [...uniqueBatch, ...existing]
          return merged.slice(0, MAX_ROWS)
        })
        return queue.slice(batch.length)
      })
    }, FLUSH_MS)
    return () => clearInterval(id)
  }, [incomingQueue.length, qc, statusFilter])

  const updateCfg = useMutation({
    mutationFn: (vars: { tempMax: number, humidityMax: number }) => api.updateConfig(vars.tempMax, vars.humidityMax),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['config'] }); setToast('Configuration updated') },
  })

  const ackAlert = useMutation({
    mutationFn: (id: string) => api.ackAlert(id),
    onSuccess: (updated) => {
      qc.setQueryData<AlertDto[]>(['alerts', statusFilter], (old) => (old || []).map(a => a.id === updated.id ? updated : a))
    }
  })

  const connectionRef = useRef<ReturnType<typeof createAlertsConnection> | null>(null)
  const lastToastTsRef = useRef<number>(0)

  useEffect(() => {
    if (connectionRef.current) return
    const connection = createAlertsConnection()
    connectionRef.current = connection

    connection.on('NewAlert', (alert: AlertDto) => {
      // queue alerts instead of immediate cache update to avoid excessive rerenders
      setIncomingQueue(prev => [...prev, alert])
      // Throttle toast to once per 1.2s; use global toaster to avoid layout shift
      const now = Date.now()
      if (now - lastToastTsRef.current > 1200) {
        notify.info(`${alert.type} alert: ${alert.value.toFixed(1)} (>${alert.threshold.toFixed(1)})`)
        lastToastTsRef.current = now
      }
    })

    let retryDelay = 1000
    const maxDelay = 15000
    let stopped = false

    const start = async () => {
      try {
        await connection.start()
        retryDelay = 1000 // reset on success
      } catch (err) {
        console.error('SignalR start error', err)
        if (stopped) return
        setTimeout(start, retryDelay)
        retryDelay = Math.min(maxDelay, Math.floor(retryDelay * 1.8))
      }
    }

    start()

    return () => {
      stopped = true
      connection.stop().catch(() => {})
      connectionRef.current = null
    }
  }, [])

  const sortedAlerts = useMemo<AlertRow[]>(() => {
    // Keep array small and precompute strings to avoid work in render
    const base = (alertsQuery.data || []).slice(0, MAX_ROWS)
    const dtf12 = new Intl.DateTimeFormat(undefined, {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: true,
      
  timeZone: 'UTC',
    })
    const prepped = base.map(a => ({
      ...a,
      // createdAtStr: dtf12.format(new Date(a.createdAt)),
      createdAtStr: new Date(a.createdAt).toLocaleString(),
      valueStr: (a.value as number).toFixed(2),
      thresholdStr: (a.threshold as number).toFixed(2),
    }))
    prepped.sort((a, b) => {
      const dir = sortDir === 'asc' ? 1 : -1
      if (sortBy === 'createdAt') return (new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()) * dir
      return ((a.value as number) - (b.value as number)) * dir
    })
    return prepped
  }, [alertsQuery.data, sortBy, sortDir])

  function logout() {
    storage.clear(); router.push('/login')
  }

  return (
    <div className="container space-y-6">
      {/* Global toaster is mounted in layout; no inline toast here to avoid layout shifts */}
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-semibold">Industrial Alerts Dashboard</h2>
        <div className="flex gap-2">
          <button className="btn-secondary" onClick={() => alertsQuery.refetch()}>Reload</button>
          <button className="btn" onClick={logout}>Logout</button>
        </div>
      </div>

      <section className="grid md:grid-cols-2 gap-4">
        <div className="card space-y-3">
          <h3 className="font-semibold">Configuration</h3>
          {cfgQuery.isLoading ? (
            <div>Loading config...</div>
          ) : cfgQuery.isError ? (
            <div className="text-red-600 text-sm">Failed to load config</div>
          ) : (
            <form onSubmit={(e) => { e.preventDefault(); updateCfg.mutate({ tempMax, humidityMax }) }} className="space-y-3">
              <div>
                <label className="label">Temp Max (°)</label>
                <input className="input" type="number" step="0.1" value={tempMax} onChange={e => setTempMax(parseFloat(e.target.value))} />
              </div>
              <div>
                <label className="label">Humidity Max (%)</label>
                <input className="input" type="number" step="0.1" value={humidityMax} onChange={e => setHumidityMax(parseFloat(e.target.value))} />
              </div>
              <div className="flex items-center gap-2">
                <button className="btn" type="submit" disabled={updateCfg.isPending}>Save</button>
                {cfgQuery.data && <span className="text-xs text-gray-500">Updated {new Date(cfgQuery.data.updatedAt).toLocaleString()}</span>}
              </div>
            </form>
          )}
        </div>

        <div className="card space-y-3">
          <h3 className="font-semibold">Filters</h3>
          <div className="flex flex-wrap items-end gap-3">
            <div>
              <label className="label">Status</label>
              <select className="input" value={statusFilter} onChange={e => setStatusFilter(e.target.value)}>
                <option value="open">Open</option>
                <option value="ack">Acknowledged</option>
                <option value="">All</option>
              </select>
            </div>
            <div>
              <label className="label">Sort by</label>
              <select className="input" value={sortBy} onChange={e => setSortBy(e.target.value as any)}>
                <option value="createdAt">Created At</option>
                <option value="value">Value</option>
              </select>
            </div>
            <div>
              <label className="label">Direction</label>
              <select className="input" value={sortDir} onChange={e => setSortDir(e.target.value as any)}>
                <option value="desc">Desc</option>
                <option value="asc">Asc</option>
              </select>
            </div>
          </div>
        </div>
      </section>

      <section className="card">
        <div className="flex items-center justify-between mb-3">
          <h3 className="font-semibold">Alerts</h3>
        </div>
        {alertsQuery.isLoading ? (
          <div>Loading alerts...</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="table">
              <thead>
                <tr>
                  <th className="th">Created</th>
                  <th className="th">Type</th>
                  <th className="th">Value</th>
                  <th className="th">Threshold</th>
                  <th className="th">Status</th>
                  <th className="th">Action</th>
                </tr>
              </thead>
              <VirtualizedTBody rows={sortedAlerts} ack={(id) => ackAlert.mutate(id)} ackPending={ackAlert.isPending} />
            </table>
          </div>
        )}
      </section>
    </div>
  )
}

const Row = memo(function Row({ a, onAck, ackPending }: { a: AlertRow, onAck: (id: string) => void, ackPending: boolean }) {
  return (
    <tr className={a.status === 'Open' ? 'bg-red-50 dark:bg-red-950/20' : ''}>
      <td className="td whitespace-nowrap">{a.createdAtStr}</td>
      <td className="td">{a.type}</td>
      <td className="td">{a.valueStr}</td>
      <td className="td">{a.thresholdStr}</td>
      <td className="td">{a.status}</td>
      <td className="td">
        {a.status === 'Open' ? (
          <button
            className="inline-flex items-center h-7 px-3 rounded-full border text-xs font-medium text-gray-700 hover:bg-gray-50 active:bg-gray-100 dark:text-gray-200 dark:border-gray-700 dark:hover:bg-gray-800 transition disabled:opacity-50 disabled:cursor-not-allowed"
            onClick={() => onAck(a.id)}
            disabled={ackPending}
          >
            Ack
          </button>
        ) : (
          <span className="text-gray-400">-</span>
        )}
      </td>
    </tr>
  )
})

function VirtualizedTBody({ rows, ack, ackPending }: { rows: AlertRow[], ack: (id: string) => void, ackPending: boolean }) {
  const parentRef = useRef<HTMLTableSectionElement | null>(null)
  const rowVirtualizer = useVirtualizer({
    count: rows.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => 40,
    overscan: 10,
  })

  const items = rowVirtualizer.getVirtualItems()

  return (
    <tbody ref={parentRef} style={{ maxHeight: 480, overflowY: 'auto' }}>
      <tr style={{ height: rowVirtualizer.getTotalSize(), position: 'relative' }}>
        <td style={{ position: 'absolute', top: 0, left: 0, right: 0 }}>
          {items.map(vi => {
            const a = rows[vi.index]
            return (
              <div key={`${a.id}-${vi.index}`} style={{ position: 'absolute', top: 0, transform: `translateY(${vi.start}px)`, width: '100%' }}>
                <table className="table" style={{ tableLayout: 'fixed' }}>
                  <tbody>
                    <Row a={a} onAck={ack} ackPending={ackPending} />
                  </tbody>
                </table>
              </div>
            )
          })}
        </td>
      </tr>
    </tbody>
  )
}


