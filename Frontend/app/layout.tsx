import './globals.css'
import React from 'react'
import Providers from './providers'
import { Toaster } from 'sonner'

export const metadata = {
  title: 'Industrial Alerts Dashboard',
  description: 'Realtime alerts and configuration',
}

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <Providers>
          {children}
        </Providers>
        <Toaster position="top-right" richColors closeButton expand />
      </body>
    </html>
  )
}




