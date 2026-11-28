import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api, type DialogListItem } from '@/services/api'

export const useDialogsStore = defineStore('dialogs', () => {
  const items = ref<DialogListItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function load() {
    loading.value = true
    error.value = null
    try {
      items.value = await api.getDialogs()
    } catch (e) {
      console.error('Не удалось загрузить список диалогов', e)
      error.value = 'Не удалось загрузить диалоги'
    } finally {
      loading.value = false
    }
  }

  /** Локально обновить «последнее сообщение», не перезапрашивая весь список. */
  function applyIncomingMessage(message: { dialogId: string; text: string; time: string }) {
    const dialog = items.value.find((d) => d.dialogId === message.dialogId)
    if (!dialog) {
      void load()
      return
    }
    dialog.lastMessageText = message.text
    dialog.lastMessageTime = message.time
    dialog.unreadCount += 1
  }

  function clearUnread(dialogId: string) {
    const dialog = items.value.find((d) => d.dialogId === dialogId)
    if (dialog) dialog.unreadCount = 0
  }

  function clear() {
    items.value = []
    error.value = null
  }

  return { items, loading, error, load, applyIncomingMessage, clearUnread, clear }
})
