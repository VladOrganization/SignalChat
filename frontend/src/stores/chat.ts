import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface Message {
  id: string
  text: string
  userName: string
  time: string
}

export const useChatStore = defineStore('chat', () => {
  const messages = ref<Message[]>([])
  const totalCount = ref(0)
  const currentPage = ref(1)
  const hasMore = computed(() => messages.value.length < totalCount.value)

  function setMessages(msgs: Message[], total: number, page: number) {
    messages.value = msgs
    totalCount.value = total
    currentPage.value = page
  }

  function appendOlderMessages(msgs: Message[], total: number, page: number) {
    const newMsgs = msgs.filter((m) => !messages.value.some((e) => e.id === m.id))
    messages.value = [...messages.value, ...newMsgs]
    totalCount.value = total
    currentPage.value = page
  }

  function addMessage(msg: Message) {
    const exists = messages.value.some((m) => m.id === msg.id)
    if (!exists) {
      messages.value = [msg, ...messages.value]
      totalCount.value++
    }
  }

  function clear() {
    messages.value = []
    totalCount.value = 0
    currentPage.value = 1
  }

  return { messages, totalCount, currentPage, hasMore, setMessages, appendOlderMessages, addMessage, clear }
})
