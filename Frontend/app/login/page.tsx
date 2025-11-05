"use client"
import React, { useState, useEffect } from 'react'
import { api } from '../../lib/api'
import { storage } from '../../lib/auth'
import { useRouter } from 'next/navigation'

export default function LoginPage() {
  const [username, setUsername] = useState('demo')
  const [password, setPassword] = useState('Password123!')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const router = useRouter()

  useEffect(() => {
    if (storage.getToken()) router.replace('/dashboard')
  }, [router])

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      const res = await api.login(username, password)
      storage.setToken(res.token)
      router.push('/dashboard')
    } catch (err: any) {
      setError(err.message || 'Login failed')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="container min-h-screen grid place-items-center">
      <form onSubmit={onSubmit} className="card w-full max-w-sm space-y-4">
        <h2 className="text-xl font-semibold">Login</h2>
        <div>
          <label className="label">Username</label>
          <input className="input" value={username} onChange={e => setUsername(e.target.value)} placeholder="demo" />
        </div>
        <div>
          <label className="label">Password</label>
          <input className="input" type="password" value={password} onChange={e => setPassword(e.target.value)} placeholder="Password123!" />
        </div>
        {error && <div className="text-red-600 text-sm">{error}</div>}
        <button className="btn w-full" disabled={loading} type="submit">{loading ? 'Signing in...' : 'Login'}</button>
      </form>
    </div>
  )
}




