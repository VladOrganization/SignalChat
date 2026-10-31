<template>
  <div class="telegram-dropdown">
    <!-- Кнопка с плюсом -->
    <button class="plus-button" @click="toggleDropdown">+</button>

    <!-- Выпадающий список -->
    <div v-if="isOpen" class="dropdown-menu">
      <div
        v-for="option in options"
        :key="option.id"
        class="dropdown-item"
        @click="selectOption(option)"
      >
        {{ option.text }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const isOpen = ref(false)

const reactionProps = defineProps({
  messageId: {
    type: String,
    required: true,
  },
})

interface ReactionOption {
  id: number
  text: string
  value: string
}

function toggleDropdown() {
  isOpen.value = !isOpen.value
}

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL as string) || 'https://localhost:7093'

async function selectOption(option: ReactionOption) {
  isOpen.value = false
  await axios.post(
    `${API_BASE_URL}/api/chat/reactions`,
    {
      messageId: reactionProps.messageId,
      reaction: option.id,
    },
    {
      headers: {
        'Authorization': `Bearer ${auth.accessToken}`,
      },
    }
  )
}

const options: ReactionOption[] = [
  { id: 1, text: '👍', value: '👍' },
  { id: 2, text: '❤️', value: '❤️' },
  { id: 3, text: '🤡', value: '🤡' },
  { id: 4, text: '💩', value: '💩' },
  { id: 5, text: '🔥', value: '🔥' },
  { id: 6, text: '👎', value: '👎' },
]
</script>

<style scoped>
.telegram-dropdown {
  position: relative;
  display: inline-block;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
}

.plus-button {
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background-color: #3390ec;
  border: none;
  color: white;
  font-size: 18px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: opacity 0.2s ease;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
}

.plus-button:hover {
  opacity: 0.85;
}

.plus-button:active {
  transform: scale(0.96);
}

.dropdown-menu {
  position: absolute;
  bottom: 100%;
  left: 0;
  margin-bottom: 8px;
  min-width: 50px;
  background-color: #ffffff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  overflow: hidden;
  z-index: 1000;
  padding: 6px 0;
}

.dropdown-item {
  padding: 12px 16px;
  font-size: 16px;
  color: #000000;
  cursor: pointer;
  transition: background-color 0.15s;
}

.dropdown-item:hover {
  background-color: #f0f2f5;
}

@media (prefers-color-scheme: dark) {
  .dropdown-menu {
    background-color: #1c1c1e;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
  }
  .dropdown-item {
    color: #ffffff;
  }
  .dropdown-item:hover {
    background-color: #2c2c2e;
  }
}
</style>