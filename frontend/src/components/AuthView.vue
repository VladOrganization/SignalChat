<template>
  <div class="auth-wrapper">
    <div class="auth-card">
      <h1>SignalChat</h1>

      <p class="hint">
        Введите <strong>имя</strong> для регистрации<br />
        или <strong>#код</strong> для входа
      </p>

      <form @submit.prevent="submit">
        <input
          v-model="input"
          type="text"
          placeholder="Имя или #код"
          autofocus
          :disabled="loading"
        />
        <p v-if="error" class="error">{{ error }}</p>
        <button type="submit" :disabled="loading || !input.trim()">
          {{ loading ? 'Подождите...' : 'Войти' }}
        </button>
      </form>

      <p v-if="registeredCode" class="code-hint">
        Ваш код для входа: <strong>{{ registeredCode }}</strong>
        <br />
        <small>Сохраните его — он понадобится для входа в следующий раз</small>
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { api } from '@/services/api'
import { useAuthStore } from '@/stores/auth'

const emit = defineEmits<{ (e: 'authenticated'): void }>()

const auth = useAuthStore()
const input = ref('')
const loading = ref(false)
const error = ref('')
const registeredCode = ref('')

async function submit() {
  const val = input.value.trim()
  if (!val) return

  loading.value = true
  error.value = ''
  registeredCode.value = ''

  try {
    let response

    if (val.startsWith('#')) {
      const code = val.slice(1).trim()
      response = await api.login(code)
    } else {
      response = await api.register(val)
      registeredCode.value = response.code
    }

    auth.setAuth(response)

    if (registeredCode.value) {
      setTimeout(() => emit('authenticated'), 2500)
    } else {
      emit('authenticated')
    }
  } catch (e: unknown) {
    error.value = e instanceof Error ? e.message : 'Произошла ошибка'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.auth-wrapper {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #1a1a2e;
}

.auth-card {
  background: #16213e;
  border-radius: 12px;
  padding: 40px;
  width: 100%;
  max-width: 380px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4);
  text-align: center;
}

h1 {
  color: #e0e0ff;
  margin: 0 0 8px;
  font-size: 1.8rem;
  letter-spacing: 2px;
}

.hint {
  color: #8888aa;
  font-size: 0.9rem;
  margin: 0 0 24px;
  line-height: 1.6;
}

form {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

input {
  padding: 12px 16px;
  border-radius: 8px;
  border: 1px solid #2a2a4a;
  background: #0f3460;
  color: #e0e0ff;
  font-size: 1rem;
  outline: none;
  transition: border-color 0.2s;
}

input:focus {
  border-color: #6c63ff;
}

input::placeholder {
  color: #5555aa;
}

button {
  padding: 12px;
  border-radius: 8px;
  border: none;
  background: #6c63ff;
  color: #fff;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}

button:hover:not(:disabled) {
  background: #7c73ff;
}

button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.error {
  color: #ff6b6b;
  font-size: 0.85rem;
  margin: 0;
  text-align: left;
}

.code-hint {
  margin-top: 20px;
  padding: 12px;
  background: #0f3460;
  border-radius: 8px;
  color: #e0e0ff;
  font-size: 0.9rem;
  line-height: 1.6;
}

.code-hint strong {
  color: #6c63ff;
  font-size: 1.1rem;
  letter-spacing: 2px;
}

small {
  color: #8888aa;
}
</style>
