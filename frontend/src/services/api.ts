import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7093'

const http = axios.create()

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

      if (!auth.refreshToken) {
        auth.logout()
        return Promise.reject(err)
      }

      try {
        const data = await api.refresh(auth.refreshToken)
        auth.setAuth(data)
        original.headers['Authorization'] = `Bearer ${data.accessToken}`
        return http(original)
      } catch (refreshErr) {
        auth.logout()
        return Promise.reject(refreshErr)
      }
    }

    return Promise.reject(err)
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
  images: string[]
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
    const params = new URLSearchParams()
    params.append('grant_type', 'refresh_token')
    params.append('refresh_token', refreshToken)

    const res = await axios.post<{
      access_token: string
      refresh_token: string
    }>(`${API_BASE_URL}/connect/token`, params, {
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    })

    return { accessToken: res.data.access_token, refreshToken: res.data.refresh_token }
  },

  async getMessages(page = 1, pageSize = 50) {
    const res = await http.get<PagedResult<MessageDto>>('/api/chat/messages', {
      params: { page, pageSize },
      headers: { 'X-Auth': '1' },
    })
    return res.data
  },

  async sendMessage(text: string, imageUrl: string[]) {
    const res = await http.post<MessageDto>(
      '/api/chat/messages',
      { text, imageUrl },
      { headers: { 'X-Auth': '1' } },
    )
    return res.data
  },
  async sendMessageImages(images: File[]) {
    const formData = new FormData()

    images.forEach((file) => {
      formData.append('file', file)
    })

    const response = await http.post<string[]>('/api/image/save', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
        'X-Auth': '1',
      },
    })

    return response.data
  },
}
