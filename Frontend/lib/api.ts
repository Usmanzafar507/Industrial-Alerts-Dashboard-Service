import { apiBase, storage } from './auth'

async function request(path: string, init?: RequestInit) {
  const token = storage.getToken()
  const headers: any = { 'Content-Type': 'application/json', ...(init?.headers || {}) }
  if (token) headers['Authorization'] = `Bearer ${token}`
  const res = await fetch(`${apiBase}${path}`, { ...init, headers })
  if (!res.ok) {
    let message = `HTTP ${res.status}`
    try { const body = await res.json(); message = body.message || JSON.stringify(body) } catch {}
    throw new Error(message)
  }
  if (res.status === 204) return null
  return res.json()
}

export const api = {
  async login(username: string, password: string): Promise<{ token: string }> {
    return request('/auth/login', { method: 'POST', body: JSON.stringify({ username, password }) })
  },
  async getConfig() {
    return request('/config')
  },
  async updateConfig(tempMax: number, humidityMax: number) {
    return request('/config', { method: 'PUT', body: JSON.stringify({ tempMax, humidityMax }) })
  },
  async listAlerts(status?: string, from?: string, to?: string) {
    const qs = new URLSearchParams()
    if (status) qs.append('status', status)
    if (from) qs.append('from', from)
    if (to) qs.append('to', to)
    const q = qs.toString() ? `?${qs}` : ''
    return request(`/alerts${q}`)
  },
  async ackAlert(id: string) {
    return request(`/alerts/${id}/ack`, { method: 'POST' })
  }
}




