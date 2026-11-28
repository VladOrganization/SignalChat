<template>
  <div class="modal-backdrop" @click.self="emit('close')">
    <div class="modal">
      <header class="modal-header">
        <h2>Новый чат</h2>
        <button class="modal-close" @click="emit('close')">×</button>
      </header>

      <input
        v-model="query"
        class="search-input"
        type="text"
        placeholder="Поиск пользователя..."
        autofocus
      />

      <div v-if="loading" class="modal-status">Поиск...</div>
      <div v-else-if="error" class="modal-status error">{{ error }}</div>
      <div v-else-if="query.trim() === ''" class="modal-status">
        Введите имя, чтобы найти собеседника
      </div>
      <div v-else-if="results.length === 0" class="modal-status">Никого не найдено</div>

      <ul v-else class="users-list">
        <li v-for="user in results" :key="user.id" class="user-item" @click="select(user)">
          <div class="user-avatar" :style="{ backgroundColor: getTelegramColor(user.userName) }">
            {{ getInitials(user.userName) }}
          </div>
          <span class="user-name">{{ user.userName }}</span>
        </li>
      </ul>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { api, type UserListItem } from '@/services/api'
import { getInitials, getTelegramColor } from '@/utils/colors'

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'selected', dialogId: string): void
}>()

const query = ref('')
const results = ref<UserListItem[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const selecting = ref(false)

let debounceTimer: ReturnType<typeof setTimeout> | null = null
let requestSeq = 0

watch(query, (value) => {
  if (debounceTimer) clearTimeout(debounceTimer)

  const term = value.trim()
  if (!term) {
    results.value = []
    loading.value = false
    error.value = null
    return
  }

  loading.value = true
  error.value = null

  debounceTimer = setTimeout(async () => {
    const seq = ++requestSeq
    try {
      const found = await api.searchUsers(term)
      if (seq !== requestSeq) return
      results.value = found
    } catch (e) {
      if (seq !== requestSeq) return
      console.error('Ошибка поиска пользователей', e)
      error.value = 'Не удалось выполнить поиск'
      results.value = []
    } finally {
      if (seq === requestSeq) loading.value = false
    }
  }, 300)
})

async function select(user: UserListItem) {
  if (selecting.value) return
  selecting.value = true
  try {
    const dialogId = await api.createDirectDialog(user.id)
    emit('selected', dialogId)
  } catch (e) {
    console.error('Не удалось создать диалог', e)
    error.value = 'Не удалось создать диалог'
  } finally {
    selecting.value = false
  }
}
</script>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.55);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
  padding: 16px;
}

.modal {
  width: 100%;
  max-width: 420px;
  max-height: 80vh;
  display: flex;
  flex-direction: column;
  background: #16213e;
  border: 1px solid #2a2a4a;
  border-radius: 14px;
  overflow: hidden;
  box-shadow: 0 16px 48px rgba(0, 0, 0, 0.45);
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 16px;
  border-bottom: 1px solid #2a2a4a;
}

.modal-header h2 {
  margin: 0;
  font-size: 1rem;
}

.modal-close {
  background: none;
  border: none;
  color: #8888aa;
  font-size: 22px;
  line-height: 1;
  cursor: pointer;
}

.modal-close:hover {
  color: #e0e0ff;
}

.search-input {
  margin: 12px 16px;
  padding: 10px 12px;
  border-radius: 8px;
  border: 1px solid #2a2a4a;
  background: #1a1a2e;
  color: #e0e0ff;
  font-size: 0.95rem;
  outline: none;
}

.search-input:focus {
  border-color: #6c63ff;
}

.modal-status {
  padding: 16px;
  color: #8888aa;
  font-size: 0.9rem;
  text-align: center;
}

.modal-status.error {
  color: #ff6b6b;
}

.users-list {
  list-style: none;
  margin: 0;
  padding: 0 8px 8px;
  overflow-y: auto;
}

.user-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px;
  border-radius: 8px;
  cursor: pointer;
}

.user-item:hover {
  background: rgba(255, 255, 255, 0.06);
}

.user-avatar {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-weight: 700;
  font-size: 0.75rem;
  flex-shrink: 0;
}

.user-name {
  font-size: 0.9rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
