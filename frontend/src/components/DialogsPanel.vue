<template>
  <aside class="dialogs-panel">
    <header class="dialogs-header">
      <span class="dialogs-title">Чаты</span>
      <button class="new-chat-btn" title="Новый чат" @click="emit('new-chat')">+</button>
    </header>

    <div v-if="dialogs.loading && dialogs.items.length === 0" class="dialogs-status">
      Загрузка...
    </div>
    <div v-else-if="dialogs.error" class="dialogs-status error">{{ dialogs.error }}</div>
    <div v-else-if="dialogs.items.length === 0" class="dialogs-status">Нет чатов</div>

    <ul v-else class="dialogs-list">
      <li
        v-for="dialog in dialogs.items"
        :key="dialog.dialogId"
        class="dialog-item"
        :class="{ active: dialog.dialogId === activeId }"
        @click="emit('select', dialog.dialogId)"
      >
        <div class="dialog-avatar" :style="{ backgroundColor: colorFor(dialog) }">
          {{ initialsFor(dialog) }}
        </div>
        <div class="dialog-body">
          <div class="dialog-top">
            <span class="dialog-name">{{ nameFor(dialog) }}</span>
            <span v-if="dialog.lastMessageTime" class="dialog-time">
              {{ formatTime(dialog.lastMessageTime) }}
            </span>
          </div>
          <div class="dialog-bottom">
            <span class="dialog-preview">{{ dialog.lastMessageText || 'Нет сообщений' }}</span>
            <span v-if="dialog.unreadCount > 0" class="dialog-badge">{{ dialog.unreadCount }}</span>
          </div>
        </div>
      </li>
    </ul>
  </aside>
</template>

<script setup lang="ts">
import { DialogType, type DialogListItem } from '@/services/api'
import { useDialogsStore } from '@/stores/dialogs'
import { getInitials, getTelegramColor } from '@/utils/colors'

defineProps<{ activeId: string | null }>()
const emit = defineEmits<{ (e: 'select', dialogId: string): void; (e: 'new-chat'): void }>()

const dialogs = useDialogsStore()

function nameFor(dialog: DialogListItem): string {
  if (dialog.type === DialogType.GlobalChat) return dialog.title ?? 'Общий чат'
  return dialog.partnerName ?? 'Без названия'
}

function colorFor(dialog: DialogListItem): string {
  return getTelegramColor(nameFor(dialog))
}

function initialsFor(dialog: DialogListItem): string {
  if (dialog.type === DialogType.GlobalChat) return 'ОЧ'
  return getInitials(dialog.partnerName)
}

function formatTime(iso: string): string {
  const date = new Date(iso)
  const now = new Date()
  const sameDay = date.toDateString() === now.toDateString()
  if (sameDay) {
    return date.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })
  }
  return date.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit' })
}
</script>

<style scoped>
.dialogs-panel {
  display: flex;
  flex-direction: column;
  width: 280px;
  min-width: 280px;
  height: 100%;
  background: #16213e;
  border-right: 1px solid #2a2a4a;
  overflow: hidden;
}

.dialogs-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 14px;
  border-bottom: 1px solid #2a2a4a;
  flex-shrink: 0;
}

.dialogs-title {
  font-weight: 700;
  font-size: 1rem;
}

.new-chat-btn {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  border: none;
  background: #6c63ff;
  color: #fff;
  font-size: 18px;
  line-height: 1;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.new-chat-btn:hover {
  background: #7c73ff;
}

.dialogs-status {
  padding: 16px;
  color: #8888aa;
  font-size: 0.9rem;
  text-align: center;
}

.dialogs-status.error {
  color: #ff6b6b;
}

.dialogs-list {
  list-style: none;
  margin: 0;
  padding: 6px;
  overflow-y: auto;
  flex: 1;
}

.dialog-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px;
  border-radius: 10px;
  cursor: pointer;
  transition: background-color 0.15s;
}

.dialog-item:hover {
  background: rgba(255, 255, 255, 0.05);
}

.dialog-item.active {
  background: rgba(108, 99, 255, 0.22);
}

.dialog-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-weight: 700;
  font-size: 0.8rem;
  flex-shrink: 0;
  user-select: none;
}

.dialog-body {
  min-width: 0;
  flex: 1;
}

.dialog-top,
.dialog-bottom {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.dialog-name {
  font-weight: 600;
  font-size: 0.9rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.dialog-time {
  font-size: 0.72rem;
  color: #8888aa;
  flex-shrink: 0;
}

.dialog-preview {
  font-size: 0.8rem;
  color: #8888aa;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.dialog-badge {
  flex-shrink: 0;
  min-width: 18px;
  height: 18px;
  padding: 0 5px;
  border-radius: 9px;
  background: #6c63ff;
  color: #fff;
  font-size: 0.7rem;
  font-weight: 600;
  display: flex;
  align-items: center;
  justify-content: center;
}

@media (max-width: 720px) {
  .dialogs-panel {
    width: 100%;
    min-width: 0;
  }
}
</style>
