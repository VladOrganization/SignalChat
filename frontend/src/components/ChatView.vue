<template>
  <div class="chat-layout">
    <header class="chat-header">
      <div class="user-info">
        <span class="username">{{ auth.userName }}</span>
        <button class="code-btn" @click="showCode = !showCode">
          #{{ showCode ? auth.code + ' 👆🙈' : '****** 👆👀' }}
        </button>
      </div>
      <button class="logout-btn" @click="logout">Выйти</button>
    </header>

    <main ref="messagesEl" class="messages" @scroll="onScroll">
      <div v-if="loading" class="status">Загрузка сообщений...</div>
      <div v-else-if="chat.messages.length === 0" class="status">
        Нет сообщений. Напишите первым!
      </div>
      <template v-else>
        <div v-if="loadingMore" class="status load-more-status">Загрузка...</div>
        <div
          v-for="msg in chat.messages"
          :key="msg.id"
          class="message"
          :class="{ own: msg.userName === auth.userName }"
        >
          <div class="bubble">
            <span  class="msg-author">{{ msg.userName }}</span>
            <span class="msg-text">{{ msg.text }}</span>
            <span class="msg-time">{{ formatTime(msg.time) }}</span>
            <img  :src="msg.imageUrl" alt="image">
          </div>
        </div>
      </template>
    </main>

    <footer class="chat-footer">
      <form @submit.prevent="send">
        <input
          v-model="text"
          type="text"
          placeholder="Введите сообщение..."
          @keydown.enter.exact.prevent="send"
        />
          <input type="file" accept="image/*" @change="handleFileChange" />
          <button @click="uploadImage" :disabled="!selectedFile">+</button>
        <button type="submit" :disabled="sending || !text.trim()|| !selectedFile">Отправить</button>
      </form>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick, watch } from 'vue'
import { api } from '@/services/api'
import { useAuthStore } from '@/stores/auth'
import { useChatStore } from '@/stores/chat'
import { startSignalR, stopSignalR } from '@/services/signalr'
import axios from 'axios'


const imagePath = ref();
//import image
// Реактивная переменная для хранения выбранного файла
const selectedFile = ref(null)

// Обработчик выбора файла
const handleFileChange = (event) => {
  selectedFile.value = event.target.files[0]
}

// Функция отправки файла на сервер
const uploadImage = async () => {
  if (!selectedFile.value) {
    alert('Выберите файл')
    return
  }

  const formData = new FormData()
  // Ключ 'image' должен совпадать с ожидаемым на сервере
  formData.append('file', selectedFile.value)

  try {
    const response = await axios.post('https://localhost:7093/api/image/save', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })

      console.log('Файл загружен:', response.data)
      return imagePath.value = response.data;



    // Дополнительная логика после успеха
  } catch (error) {
    console.error('Ошибка загрузки:', error)
    alert('Не удалось загрузить изображение')
  }
}



//end  import image
const emit = defineEmits<{ (e: 'logout'): void }>()

const auth = useAuthStore()
const chat = useChatStore()
//const itogUrl = 'https://localhost:7093/'+imagePath.value;
const showCode = ref(false)
const text = ref('')
const sending = ref(false)
const loading = ref(true)
const loadingMore = ref(false)
const messagesEl = ref<HTMLElement | null>(null)

function formatTime(iso: string) {
  const d = new Date(iso)
  return d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })
}

async function scrollToBottom() {
  await nextTick()
  if (messagesEl.value) {
    messagesEl.value.scrollTop = messagesEl.value.scrollHeight
  }
}

// Scroll to bottom only when a new SignalR message arrives (messages[0] changes)
watch(
  () => chat.messages[0]?.id,
  (newId, oldId) => {
    if (newId && newId !== oldId) {
      scrollToBottom()
    }
  },
)

async function loadMore() {
  if (!chat.hasMore || loadingMore.value) return
  loadingMore.value = true

  const el = messagesEl.value!
  const prevScrollHeight = el.scrollHeight
  const prevScrollTop = el.scrollTop

  try {
    const result = await api.getMessages(chat.currentPage + 1)
    chat.appendOlderMessages(result.items, result.totalCount, result.page)
    await nextTick()
    el.scrollTop = prevScrollTop + (el.scrollHeight - prevScrollHeight)
  } finally {
    loadingMore.value = false
  }
}

function onScroll() {
  const el = messagesEl.value
  if (!el || loadingMore.value || !chat.hasMore) return
  if (el.scrollTop <= 100) {
    loadMore()
  }
}

async function send() {

  const val = text.value.trim()
  if (!val || sending.value) return
  sending.value = true
  try {
    await api.sendMessage(val,`https://localhost:7093/${imagePath.value}`)
    text.value = ''
    imagePath.value = ''
  } catch {
    // message arrives via SignalR so just clear on success
  } finally {
    sending.value = false
  }
}

function logout() {
  stopSignalR()
  chat.clear()
  auth.logout()
  emit('logout')
}

onMounted(async () => {
  try {
    const result = await api.getMessages()
    chat.setMessages(result.items, result.totalCount, result.page)
    await scrollToBottom()
  } finally {
    loading.value = false
  }

  await startSignalR()
})

onUnmounted(() => {
  stopSignalR()
})
</script>

<style scoped>
.chat-layout {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background: #1a1a2e;
  color: #e0e0ff;
}

.chat-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 20px;
  background: #16213e;
  border-bottom: 1px solid #2a2a4a;
  flex-shrink: 0;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 10px;
}

.username {
  font-weight: 600;
  font-size: 1rem;
}

.code-btn {
  padding: 6px 14px;
  border-radius: 6px;
  border: 1px solid #2a2a4a;
  background: transparent;
  color: #8888aa;
  cursor: pointer;
  font-size: 0.85rem;
  transition: all 0.2s;
}

.logout-btn {
  padding: 6px 14px;
  border-radius: 6px;
  border: 1px solid #2a2a4a;
  background: transparent;
  color: #8888aa;
  cursor: pointer;
  font-size: 0.85rem;
  transition: all 0.2s;
}

.logout-btn:hover {
  border-color: #ff6b6b;
  color: #ff6b6b;
}

.messages {
  flex: 1;
  overflow-y: auto;
  overflow-anchor: none;
  padding: 20px;
  display: flex;
  flex-direction: column-reverse;
  gap: 10px;
}

.status {
  text-align: center;
  color: #5555aa;
  margin: auto;
}

.load-more-status {
  margin: 0 0 8px;
  font-size: 0.85rem;
}

.message {
  display: flex;
}

.message.own {
  justify-content: flex-end;
}

.bubble {
  max-width: 70%;
  background: #16213e;
  border-radius: 12px 12px 12px 2px;
  padding: 10px 14px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.message.own .bubble {
  background: #0f3460;
  border-radius: 12px 12px 2px 12px;
}

.msg-author {
  font-size: 0.75rem;
  color: #6c63ff;
  font-weight: 600;
}

.message.own .msg-author {
  display: none;
}

.msg-text {
  font-size: 0.95rem;
  word-break: break-word;
  line-height: 1.4;
}

.msg-time {
  font-size: 0.7rem;
  color: #5555aa;
  align-self: flex-end;
}

.chat-footer {
  padding: 14px 20px;
  background: #16213e;
  border-top: 1px solid #2a2a4a;
  flex-shrink: 0;
}

.chat-footer form {
  display: flex;
  gap: 10px;
}

.chat-footer input {
  flex: 1;
  padding: 10px 16px;
  border-radius: 8px;
  border: 1px solid #2a2a4a;
  background: #1a1a2e;
  color: #e0e0ff;
  font-size: 0.95rem;
  outline: none;
  transition: border-color 0.2s;
}

.chat-footer input:focus {
  border-color: #6c63ff;
}

.chat-footer input::placeholder {
  color: #5555aa;
}

.chat-footer button {
  padding: 10px 20px;
  border-radius: 8px;
  border: none;
  background: #6c63ff;
  color: #fff;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
  white-space: nowrap;
}

.chat-footer button:hover:not(:disabled) {
  background: #7c73ff;
}

.chat-footer button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
