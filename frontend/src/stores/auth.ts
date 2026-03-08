import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore(
  'auth',
  () => {
    const id = ref<string | null>(null)
    const userName = ref<string | null>(null)
    const code = ref<string | null>(null)
    const accessToken = ref<string | null>(null)
    const refreshToken = ref<string | null>(null)

    const isAuthenticated = computed(() => !!accessToken.value)

    function setAuth(data: {
      id: string
      userName: string
      code: string
      accessToken: string
      refreshToken: string
    }) {
      id.value = data.id
      userName.value = data.userName
      code.value = data.code
      accessToken.value = data.accessToken
      refreshToken.value = data.refreshToken
    }

    function logout() {
      id.value = null
      userName.value = null
      code.value = null
      accessToken.value = null
      refreshToken.value = null
    }

    return { id, userName, code, accessToken, refreshToken, isAuthenticated, setAuth, logout }
  },
  {
    persist: true,
  },
)
