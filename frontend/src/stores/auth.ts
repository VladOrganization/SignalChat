import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore(
  'auth',
  () => {
    const accessToken = ref<string | null>(null)
    const refreshToken = ref<string | null>(null)
    const userName = ref<string>('Пользователь')
    const userId = ref<string | null>(null)

    const isAuthenticated = computed(() => !!accessToken.value)

    function setAuth(data: {
      accessToken: string
      refreshToken: string
      userName?: string
      userId?: string
    }) {
      accessToken.value = data.accessToken
      refreshToken.value = data.refreshToken
      if (data.userName) {
        userName.value = data.userName
      }
      if (data.userId) {
        userId.value = data.userId
      }
    }

    function setUserName(name: string) {
      const trimmed = name.trim()
      if (trimmed) {
        userName.value = trimmed
      }
    }

    function setUser(data: { id?: string; userName?: string }) {
      if (data.id) {
        userId.value = data.id
      }
      if (data.userName) {
        setUserName(data.userName)
      }
    }

    function logout() {
      accessToken.value = null
      refreshToken.value = null
      userName.value = 'Пользователь'
      userId.value = null
    }

    return {
      accessToken,
      refreshToken,
      userName,
      userId,
      isAuthenticated,
      setAuth,
      setUserName,
      setUser,
      logout,
    }
  },
  {
    persist: true,
  },
)
