<template>
  <div ref="rootRef" class="reaction-picker">
    <button
      ref="buttonRef"
      type="button"
      class="plus-button"
      title="Добавить реакцию"
      aria-haspopup="menu"
      :aria-expanded="isOpen"
      @click.stop="toggleDropdown"
    >
      +
    </button>

    <Teleport to="body">
      <Transition name="reaction-pop">
        <div
          v-if="isOpen"
          ref="menuRef"
          class="dropdown-menu"
          role="menu"
          aria-label="Выбор реакции"
          :style="menuStyle"
          @click.stop
        >
          <button
            v-for="option in options"
            :key="option.id"
            type="button"
            class="dropdown-item"
            role="menuitem"
            :title="option.label"
            :disabled="sending"
            @click="selectOption(option)"
          >
            {{ option.text }}
          </button>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, ref } from 'vue'
import type { CSSProperties } from 'vue'
import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const isOpen = ref(false)
const sending = ref(false)

const rootRef = ref<HTMLElement | null>(null)
const buttonRef = ref<HTMLButtonElement | null>(null)
const menuRef = ref<HTMLElement | null>(null)

// Размеры нужны только как запасной вариант до первого измерения меню.
const FALLBACK_WIDTH = 286
const FALLBACK_HEIGHT = 52
const EDGE_GAP = 8

const position = ref({ top: 0, left: 0 })

const menuStyle = computed<CSSProperties>(() => ({
  top: `${position.value.top}px`,
  left: `${position.value.left}px`,
}))

const reactionProps = defineProps({
  messageId: {
    type: String,
    required: true,
  },
})

const emit = defineEmits<{ (e: 'reacted', reactionId: number): void }>()

interface ReactionOption {
  id: number
  text: string
  label: string
}

function closeDropdown() {
  isOpen.value = false
  removeListeners()
}

async function updatePosition() {
  const button = buttonRef.value
  if (!button) return

  const rect = button.getBoundingClientRect()
  const width = menuRef.value?.offsetWidth || FALLBACK_WIDTH
  const height = menuRef.value?.offsetHeight || FALLBACK_HEIGHT

  // Центрируем меню над кнопкой и не даём ему выйти за края окна.
  const idealLeft = rect.left + rect.width / 2 - width / 2
  const maxLeft = window.innerWidth - width - EDGE_GAP
  const left = Math.max(EDGE_GAP, Math.min(idealLeft, maxLeft))

  let top = rect.top - height - EDGE_GAP
  if (top < EDGE_GAP) {
    top = Math.min(rect.bottom + EDGE_GAP, window.innerHeight - height - EDGE_GAP)
  }

  position.value = { top, left }
}

async function toggleDropdown() {
  if (isOpen.value) {
    closeDropdown()
    return
  }

  isOpen.value = true
  await nextTick()
  if (!isOpen.value) return
  await updatePosition()
  if (!isOpen.value) return
  document.addEventListener('pointerdown', onPointerDown)
  window.addEventListener('scroll', closeDropdown, true)
  window.addEventListener('resize', closeDropdown)
  document.addEventListener('keydown', onKeyDown)
}

function removeListeners() {
  document.removeEventListener('pointerdown', onPointerDown)
  window.removeEventListener('scroll', closeDropdown, true)
  window.removeEventListener('resize', closeDropdown)
  document.removeEventListener('keydown', onKeyDown)
}

function onPointerDown(event: PointerEvent) {
  const target = event.target as Node | null
  if (!target) return
  if (rootRef.value?.contains(target) || menuRef.value?.contains(target)) return
  closeDropdown()
}

function onKeyDown(event: KeyboardEvent) {
  if (event.key === 'Escape') closeDropdown()
}

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL as string) || 'https://localhost:7093'

async function selectOption(option: ReactionOption) {
  if (sending.value) return
  sending.value = true

  try {
    await axios.post(
      `${API_BASE_URL}/api/chat/reactions`,
      {
        messageId: reactionProps.messageId,
        reaction: option.id,
      },
      {
        headers: {
          Authorization: `Bearer ${auth.accessToken}`,
        },
      },
    )
    emit('reacted', option.id)
    closeDropdown()
  } catch (error) {
    console.error('Не удалось поставить реакцию:', error)
  } finally {
    sending.value = false
  }
}

const options: ReactionOption[] = [
  { id: 1, text: '👍', label: 'Нравится' },
  { id: 2, text: '❤️', label: 'Любовь' },
  { id: 3, text: '🤡', label: 'Клоун' },
  { id: 4, text: '💩', label: 'Не нравится' },
  { id: 5, text: '🔥', label: 'Огонь' },
  { id: 6, text: '👎', label: 'Дизлайк' },
]

onBeforeUnmount(removeListeners)
</script>

<style scoped>
.reaction-picker {
  display: inline-flex;
  align-items: center;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
}

.plus-button {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background-color: #3390ec;
  border: none;
  color: #fff;
  font-size: 18px;
  font-weight: 500;
  line-height: 1;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
  transition:
    opacity 0.2s ease,
    transform 0.15s ease;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
}

.plus-button:hover {
  opacity: 0.85;
}

.plus-button:active {
  transform: scale(0.94);
}

.dropdown-menu {
  position: fixed;
  z-index: 10000;
  display: flex;
  align-items: center;
  gap: 4px;
  max-width: calc(100vw - 16px);
  padding: 6px;
  background-color: #fff;
  border-radius: 999px;
  box-shadow:
    0 8px 24px rgba(0, 0, 0, 0.18),
    0 2px 6px rgba(0, 0, 0, 0.08);
}

.dropdown-item {
  width: 38px;
  height: 38px;
  padding: 0;
  border: none;
  border-radius: 50%;
  background: transparent;
  font-size: 22px;
  line-height: 1;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition:
    background-color 0.15s ease,
    transform 0.15s ease;
}

.dropdown-item:hover:not(:disabled) {
  background-color: #f0f2f5;
  transform: scale(1.12);
}

.dropdown-item:active:not(:disabled) {
  transform: scale(0.96);
}

.dropdown-item:disabled {
  opacity: 0.55;
  cursor: default;
}

.dropdown-item:focus-visible {
  outline: 2px solid #3390ec;
  outline-offset: 1px;
}

.reaction-pop-enter-active,
.reaction-pop-leave-active {
  transition:
    opacity 0.15s ease,
    transform 0.15s ease;
  transform-origin: center bottom;
}

.reaction-pop-enter-from,
.reaction-pop-leave-to {
  opacity: 0;
  transform: translateY(4px) scale(0.92);
}

@media (prefers-color-scheme: dark) {
  .dropdown-menu {
    background-color: #1c1c1e;
    box-shadow:
      0 8px 24px rgba(0, 0, 0, 0.45),
      0 2px 6px rgba(0, 0, 0, 0.3);
  }

  .dropdown-item:hover:not(:disabled) {
    background-color: #2c2c2e;
  }
}

@media (prefers-reduced-motion: reduce) {
  .reaction-pop-enter-active,
  .reaction-pop-leave-active,
  .plus-button,
  .dropdown-item {
    transition: none;
  }
}
</style>
