import * as signalR from '@microsoft/signalr'
import { useAuthStore } from '@/stores/auth'
import { useChatStore, type Message } from '@/stores/chat'

let connection: signalR.HubConnection | null = null

export async function startSignalR() {
  const auth = useAuthStore()
  const chat = useChatStore()

  if (!auth.accessToken) return

  if (connection && connection.state === signalR.HubConnectionState.Connected) {
    return
  }

  connection = new signalR.HubConnectionBuilder()
    .withUrl('https://localhost:7093/chathub', {
      accessTokenFactory: () => auth.accessToken || '',
    })
    .withAutomaticReconnect()
    .build()

  connection.on('ReceiveMessage', (message: unknown) => {
    chat.addMessage(message as Message)
  })

  connection.on('ReceiveReaction', (reactionData: unknown) => {
    console.log('Reaction received:', reactionData)
  })

  try {
    await connection.start()
    console.log('SignalR Connected.')
  } catch (err) {
    console.error('SignalR Connection Error: ', err)
    setTimeout(startSignalR, 5000)
  }
}

export function stopSignalR() {
  if (connection) {
    connection.stop()
    connection = null
    console.log('SignalR Disconnected.')
  }
}