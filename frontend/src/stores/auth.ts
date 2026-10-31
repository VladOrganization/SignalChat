import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore(
  'auth',
  () => {
    const accessToken = ref<string | null>(null)
    const refreshToken = ref<string | null>(null)

    const isAuthenticated = computed(() => !!accessToken.value)

    function setAuth(data: {
      accessToken: string
      refreshToken: string
    }) {
      accessToken.value = data.accessToken
      refreshToken.value = data.refreshToken
    }

    function logout() {
      accessToken.value = null
      refreshToken.value = null
    }

    return {  accessToken, refreshToken, isAuthenticated, setAuth, logout }
  },
  {
    persist: true,
  },
)
