import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore(
  'auth',
  () => {
    const accessToken = ref<string | null>(null)
    const refreshToken = ref<string | null>(null)
    const userName = ref<string>('Пользователь')

    const isAuthenticated = computed(() => !!accessToken.value)

    function setAuth(data: {
      accessToken: string
      refreshToken: string
      userName?: string
    }) {
      accessToken.value = data.accessToken
      refreshToken.value = data.refreshToken
      if (data.userName) {
        userName.value = data.userName
      }
    }

    function logout() {
      accessToken.value = null
      refreshToken.value = null
      userName.value = 'Пользователь'
    }

    return { accessToken, refreshToken, userName, isAuthenticated, setAuth, logout }
  },
  {
    persist: true,
  },
)