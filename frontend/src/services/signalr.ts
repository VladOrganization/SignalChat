import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr'
import { useAuthStore } from '@/stores/auth'
import { useChatStore } from '@/stores/chat'
import type { Message } from '@/stores/chat'

let connection: HubConnection | null = null

export async function startSignalR() {
  const auth = useAuthStore()

  connection = new HubConnectionBuilder()
    .withUrl(`${import.meta.env.VITE_API_BASE_URL}/hubs/chat`, {
      accessTokenFactory: () => auth.accessToken ?? '',
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()

  const chat = useChatStore()

  connection.on('ReceiveMessage', (msg: Message) => {
    chat.addMessage(msg)
  })

  await connection.start()
}

export async function stopSignalR() {
  if (connection && connection.state !== HubConnectionState.Disconnected) {
    await connection.stop()
  }
  connection = null
}
