<template>
  <div class="auth-wrapper">
    <div class="auth-card">
      <h1>SignalChat</h1>

      <div class="flip-container" :class="{ flipped: isLogin }">
        <!-- Сторона регистрации (front) -->
        <div class="flip-card front">
          <form @submit.prevent="handleSubmit">
            <p class="hint">Создайте аккаунт, заполнив поля</p>
            <input
              v-model="registerData.userName"
              type="text"
              placeholder="Имя пользователя"
              autofocus
              :disabled="loading"
            />
            <input
              v-model="registerData.email"
              type="email"
              placeholder="Email"
              :disabled="loading"
            />
            <input
              v-model="registerData.password"
              type="password"
              placeholder="Пароль"
              :disabled="loading"
            />
            <p v-if="error" class="error">{{ error }}</p>
            <button type="submit" :disabled="loading || !isRegisterFormValid">
              {{ loading ? 'Подождите...' : 'Зарегистрироваться' }}
            </button>
            <p v-if="registeredCode" class="code-hint">
              Ваш код для входа: <strong>{{ registeredCode }}</strong>
              <br />
              <small>Сохраните его — он понадобится для входа в следующий раз</small>
            </p>
          </form>
          <div class="toggle-link">
            <span>Уже есть аккаунт?</span>
            <button type="button" @click="toggleMode">Войти</button>
          </div>
        </div>

        <!-- Сторона входа (back) -->
        <div class="flip-card back">
          <form @submit.prevent="handleSubmit">
            <p class="hint">Введите данные для входа</p>
            <input
              v-model="loginData.email"
              type="email"
              placeholder="Email"
              autofocus
              :disabled="loading"
            />
            <input
              v-model="loginData.password"
              type="password"
              placeholder="Пароль"
              :disabled="loading"
            />
            <p v-if="error" class="error">{{ error }}</p>
            <button type="submit" :disabled="loading || !isLoginFormValid">
              {{ loading ? 'Подождите...' : 'Войти' }}
            </button>
          </form>
          <div class="toggle-link">
            <span>Нет аккаунта?</span>
            <button type="button" @click="toggleMode">Зарегистрироваться</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import axios from 'axios'
import { useAuthStore } from '@/stores/auth';
const authStore = useAuthStore()
// Если используете Pinia / Vuex — раскомментируйте и настройте
// import { useAuthStore } from '@/stores/auth'

const emit = defineEmits<{ (e: 'authenticated'): void }>()

// ========== Конфигурация из переменных окружения ==========
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7093'
// const OAUTH_CLIENT_SECRET = import.meta.env.VITE_OAUTH_CLIENT_SECRET || '' // если требуется

// ========== Реактивные данные ==========
const isLogin = ref(false)

const registerData = ref({
  userName: '',
  email: '',
  password: '',
})

const loginData = ref({
  email: '',
  password: '',
})

const loading = ref(false)
const error = ref('')
const registeredCode = ref('')

// ========== Валидация форм ==========
const isRegisterFormValid = computed(() => {
  const { userName, email, password } = registerData.value
  return userName.trim() && email.trim() && password.trim()
})

const isLoginFormValid = computed(() => {
  const { email, password } = loginData.value
  return email.trim() && password.trim()
})

// ========== Переключение режима ==========
function toggleMode() {
  isLogin.value = !isLogin.value
  error.value = ''
  if (isLogin.value) {
    registerData.value = { userName: '', email: '', password: '' }
  } else {
    loginData.value = { email: '', password: '' }
  }
  registeredCode.value = ''
}

// ========== Общий обработчик отправки ==========
async function handleSubmit() {
  error.value = ''
  registeredCode.value = ''

  if (isLogin.value) {
    await login()
  } else {
    await register()
  }
}

// ========== Регистрация (без изменений) ==========
async function register() {
  const { userName, email, password } = registerData.value
  if (!userName.trim() || !email.trim() || !password.trim()) return

  loading.value = true
  try {
    const response = await axios.post(`${API_BASE_URL}/register`, {
      userName: userName.trim(),
      email: email.trim(),
      password: password.trim(),
    })
    if (response.data?.code) {
      registeredCode.value = response.data.code
    } else {
      emit('authenticated')
    }
    registerData.value = { userName: '', email: '', password: '' }
  } catch (e: unknown) {
    error.value = e instanceof Error ? e.message : 'Ошибка регистрации'
  } finally {
    loading.value = false
  }
}

// ============================================================
// 🚀 УЛУЧШЕННЫЙ МЕТОД ВХОДА ЧЕРЕЗ OPENIDDICT (/connect/token)
// ============================================================
async function login() {
  const { email, password } = loginData.value
  if (!email.trim() || !password.trim()) return

  loading.value = true
  error.value = ''

  try {
    // 1. Формируем тело запроса в формате x-www-form-urlencoded
    const params = new URLSearchParams()
    params.append('grant_type', 'password')
    params.append('username', email.trim())
    params.append('password', password.trim())
  
    const response = await axios.post<{
      access_token: string
      refresh_token: string
      expires_in: number
      token_type: string
    }>(
      `${API_BASE_URL}/connect/token`,
      params,
      {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
      }
    )

    // 3. Извлекаем токены
    const accessToken = response.data.access_token
    const refreshToken:string = response.data.refresh_token

    authStore.setAuth({accessToken,refreshToken});
    // 5. Устанавливаем заголовок Authorization для всех последующих запросов (глобально)
    axios.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`
    // 6. Сигнализируем родителю об успешном входе
    emit('authenticated')
    
    // 7. Очищаем форму
    loginData.value = { email: '', password: '' }
  } catch (e: unknown) {
    if (axios.isAxiosError(e) && e.response) {
      // OpenIddict возвращает ошибку в полях error или error_description
      const errorData = e.response.data as { error?: string; error_description?: string }
      error.value = errorData.error_description || errorData.error || 'Неверный email или пароль'
    } else {
      error.value = e instanceof Error ? e.message : 'Ошибка соединения с сервером'
    }
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
/* ===== Стили (без изменений) ===== */
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
  perspective: 1000px;
}

h1 {
  color: #e0e0ff;
  margin: 0 0 8px;
  font-size: 1.8rem;
  letter-spacing: 2px;
}

.flip-container {
  position: relative;
  width: 100%;
  min-height: 400px;
  transition: transform 0.6s ease;
  transform-style: preserve-3d;
}

.flip-container.flipped {
  transform: rotateY(180deg);
}

.flip-card {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  backface-visibility: hidden;
  -webkit-backface-visibility: hidden;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.flip-card.front {
  z-index: 2;
  transform: rotateY(0deg);
}

.flip-card.back {
  transform: rotateY(180deg);
}

form {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.hint {
  color: #8888aa;
  font-size: 0.9rem;
  margin: 0 0 8px;
  line-height: 1.6;
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

button[type="submit"] {
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

button[type="submit"]:hover:not(:disabled) {
  background: #7c73ff;
}

button[type="submit"]:disabled {
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
  margin-top: 12px;
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

.toggle-link {
  margin-top: 16px;
  font-size: 0.9rem;
  color: #8888aa;
}

.toggle-link button {
  background: none;
  border: none;
  color: #6c63ff;
  font-weight: 600;
  cursor: pointer;
  padding: 0 4px;
  font-size: 0.9rem;
  text-decoration: underline;
  transition: color 0.2s;
}

.toggle-link button:hover {
  color: #7c73ff;
}
</style>