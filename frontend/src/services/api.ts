import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

const http = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
})

http.interceptors.request.use((config) => {
  if (config.headers['X-Auth']) {
    const auth = useAuthStore()
    if (auth.accessToken) {
      config.headers['Authorization'] = `Bearer ${auth.accessToken}`
    }
    delete config.headers['X-Auth']
  }
  return config
})

http.interceptors.response.use(
  (res) => res,
  async (err) => {
    const original = err.config
    const auth = useAuthStore()

    if (err.response?.status === 401 && !original._retry) {
      original._retry = true

      await api
        .refresh(auth.refreshToken!)
        .then((data) => {
          auth.setAuth(data)
          original.headers['Authorization'] = `Bearer ${data.accessToken}`
        })
        .catch((err) => {
          useAuthStore().logout()
        })

      return http(original)
    }

    useAuthStore().logout()
  },
)

export interface AuthResponse {
  id: string
  userName: string
  code: string
  accessToken: string
  refreshToken: string
}

export interface MessageDto {
  id: string
  text: string
  userName: string
  time: string
  imageUrl:string,
  reaction:number
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export const api = {
  async register(userName: string) {
    const res = await http.post<AuthResponse>('/api/auth/register', { userName })
    return res.data
  },

  async login(code: string) {
    const res = await http.post<AuthResponse>('/api/auth/login', { code })
    return res.data
  },

  async refresh(refreshToken: string) {
    const res = await http.post<AuthResponse>('/api/auth/refresh', { refreshToken })
    return res.data
  },

  async getMessages(page = 1, pageSize = 50) {
    const res = await http.get<PagedResult<MessageDto>>('/api/chat/messages', {
      params: { page, pageSize },
      headers: { 'X-Auth': '1' },
    })
    return res.data
  },

  async sendMessage(text: string,imageUrl:string) {
    const res = await http.post<MessageDto>(
      '/api/chat/messages',
      { text ,imageUrl},
      { headers: { 'X-Auth': '1' } },
    )
    return res.data
  },
  async sendMessageImage(imageUrl: string) {
    const res = await http.post<MessageDto>(
      '/api/chat/images',
      {imageUrl },
      { headers: { 'X-Auth': '1' } },
    )
    return res.data
  },
  async sendReactionMessage(reaction: number,messageId:string) {
    const res = await http.post<MessageDto>(
      '/api/chat/images',
      {reaction,messageId },
      { headers: { 'X-Auth': '1' } },
    )
    return res.data
  }
}
