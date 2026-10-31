<template>
  <div class="chat-layout">
    <header class="chat-header">
      <div class="user-info">
        <div 
          class="user-avatar"
          :style="{ backgroundColor: getTelegramColor(auth.userName) }"
        >
          {{ getInitials(auth.userName) }}
        </div>
        <span class="username">{{ auth.userName }}</span>
      </div>
      <button class="logout-btn" @click="logout">Выйти</button>
    </header>

    <main ref="messages" class="messages" @scroll="onScroll">
      <div v-if="loading" class="status">Загрузка сообщений...</div>
      <div v-else-if="chat.messages.length === 0" class="status">
        Нет сообщений. Напишите первым!
      </div>
      <template v-else>
        <div v-if="loadingMore" class="status load-more-status">Загрузка...</div>
        <MessageBubble
          v-for="msg in chat.messages"
          :key="msg.id"
          :message="{
            id: msg.id,
            text: msg.text,
            time: formatTime(msg.time),
            own: msg.userName === auth.userName,
            senderName: msg.userName,
            images: msg.images,
            reactions: msg.reactions
          }"
        />
      </template>
    </main>

    <footer class="chat-footer">
      <ImagePreviewGrid :images @remove="removeFile" />
      <form @submit.prevent="send">
        <input
          v-model.trim="textModel"
          type="text"
          placeholder="Введите сообщение..."
          @keydown.enter.exact.prevent="send"
        />
        <ImageUploadButton @select="addFiles" :disabled="images.length >= 9" />
        <button type="submit" :disabled="sending || (!textModel && images.length == 0)">
          Отправить
        </button>
      </form>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick, watch, useTemplateRef } from 'vue'
import { api } from '@/services/api'
import { useAuthStore } from '@/stores/auth'
import { useChatStore } from '@/stores/chat'
import { startSignalR, stopSignalR } from '@/services/signalr'
import ImageUploadButton from '@/components/ImageUploadButton.vue'
import ImagePreviewGrid from '@/components/ImagePreviewGrid.vue'
import MessageBubble from '@/components/MessageBubble.vue'
import { getTelegramColor, getInitials } from '@/utils/colors' // Убедитесь, что путь к colors/color правильный

export interface ImageFile {
  id: string
  name: string
  url: string
  file: File
}

const images = ref<ImageFile[]>([])
let idCounter = 0

function addFiles(files: File[]) {
  for (const file of files) {
    if (images.value.length >= 9) return
    const url = URL.createObjectURL(file)
    images.value.push({ id: String(idCounter++), name: file.name, url, file })
  }
}

function removeFile(index: number) {
  const [removed] = images.value.splice(index, 1)
  if (removed) URL.revokeObjectURL(removed.url)
}

const emit = defineEmits<{ (e: 'logout'): void }>()
const auth = useAuthStore()
const chat = useChatStore()
const textModel = ref('')
const sending = ref(false)
const loading = ref(true)
const loadingMore = ref(false)
const messagesRef = useTemplateRef('messages')

function formatTime(iso: string) {
  if (!iso) return ''
  const d = new Date(iso)
  return d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })
}

async function scrollToBottom() {
  await nextTick()
  if (messagesRef.value) {
    messagesRef.value.scrollTop = messagesRef.value.scrollHeight
  }
}

watch(
  () => chat.messages[0]?.id,
  (newId, oldId) => {
    if (newId && newId !== oldId) scrollToBottom()
  }
)

async function loadMore() {
  if (!chat.hasMore || loadingMore.value) return
  loadingMore.value = true
  const el = messagesRef.value!
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
  const el = messagesRef.value
  if (!el || loadingMore.value || !chat.hasMore) return
  if (el.scrollTop <= 100) loadMore()
}

async function send() {
  const val = textModel.value
  let imagesPath: string[] = []
  if (images.value.length > 0) {
    sending.value = true
    imagesPath = (await loadImages()) as string[]
  }

  if (!val && imagesPath.length == 0) return

  sending.value = true
  try {
    await api.sendMessage(val, imagesPath)
    textModel.value = ''
    images.value = []
  } finally {
    sending.value = false
  }
}

async function loadImages() {
  return await api.sendMessageImages(images.value.map((i) => i.file))
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
  width: 100%;
  height: 100vh;
  height: 100dvh;
  background: #1a1a2e;
  color: #e0e0ff;
  overflow: hidden;
}

.chat-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 16px;
  background: #16213e;
  border-bottom: 1px solid #2a2a4a;
  flex-shrink: 0;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 0;
}

.user-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 0.85rem;
  color: #fff;
  flex-shrink: 0;
}

.username {
  font-weight: 600;
  font-size: 0.95rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.logout-btn {
  padding: 6px 12px;
  border-radius: 6px;
  border: 1px solid #2a2a4a;
  background: transparent;
  color: #8888aa;
  cursor: pointer;
  font-size: 0.85rem;
  transition: all 0.2s;
  flex-shrink: 0;
}

.logout-btn:hover {
  border-color: #ff6b6b;
  color: #ff6b6b;
}

.messages {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 16px;
  display: flex;
  flex-direction: column-reverse;
  gap: 8px;
  width: 100%;
}

.status {
  text-align: center;
  color: #5555aa;
  margin: auto;
}

.chat-footer {
  padding: 10px 12px;
  background: #16213e;
  border-top: 1px solid #2a2a4a;
  flex-shrink: 0;
  width: 100%;
}

.chat-footer form {
  display: flex;
  gap: 8px;
  align-items: center;
  width: 100%;
}

.chat-footer input {
  flex: 1;
  min-width: 0;
  padding: 10px 14px;
  border-radius: 8px;
  border: 1px solid #2a2a4a;
  background: #1a1a2e;
  color: #e0e0ff;
  font-size: 0.95rem;
  outline: none;
}

.chat-footer input:focus {
  border-color: #6c63ff;
}

.chat-footer button {
  padding: 10px 14px;
  border-radius: 8px;
  border: none;
  background: #6c63ff;
  color: #fff;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  flex-shrink: 0;
}

/* Мобильная адаптивность */
@media (max-width: 480px) {
  .chat-header {
    padding: 8px 12px;
  }
  .messages {
    padding: 10px 8px;
  }
  .chat-footer {
    padding: 8px;
  }
  .chat-footer button {
    padding: 10px 10px;
    font-size: 0.85rem;
  }
}
</style>