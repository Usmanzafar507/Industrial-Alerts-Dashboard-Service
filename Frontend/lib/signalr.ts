import * as signalR from '@microsoft/signalr'
import { apiBase, storage } from './auth'

export function createAlertsConnection() {
  const token = storage.getToken()
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${apiBase}/hubs/alerts`, {
      accessTokenFactory: () => token || ''
    })
    .withAutomaticReconnect()
    .build()
  return connection
}




