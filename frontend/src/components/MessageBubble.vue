<template>
  <div
    class="message-wrapper"
    :class="[message.own ? 'own' : 'other']"
  >
    <!-- Аватарка отправителя для чужих сообщений -->
    <div
      v-if="!message.own"
      class="msg-avatar"
      :style="{ backgroundColor: getTelegramColor(message.senderName) }"
    >
      {{ getInitials(message.senderName) }}
    </div>

    <div
      class="message-bubble"
      :class="[message.own ? 'own' : 'other', { 'has-images': imageCount > 0 }]"
    >
      <div class="message-content">
        <!-- Имя отправителя над сообщением -->
        <div
          v-if="!message.own && message.senderName"
          class="msg-author-tg"
          :style="{ color: getTelegramColor(message.senderName) }"
        >
          {{ message.senderName }}
        </div>

        <div class="message-text" v-if="message.text">{{ message.text }}</div>

        <!-- Сетка изображений -->
        <div v-if="imageCount > 0" class="image-grid" :class="gridClass">
          <div v-for="(img, i) in message.images" :key="i" class="grid-item" :class="itemClass()">
            <img :src="`${baseURL}/${img}`" loading="lazy" alt="" />
          </div>
        </div>

        <!-- Реакции -->
        <div v-if="message.reactions" class="reactions-container">
          <template v-if="Array.isArray(message.reactions)">
            <div
              v-for="(r, idx) in message.reactions"
              :key="idx"
              class="reaction-pill"
            >
              <span class="reaction-emoji">{{ getReactionEmoji(r.reactionEnum) }}</span>
              <span class="reaction-count" v-if="Number(r.count) > 0">{{ r.count }}</span>
            </div>
          </template>
          <template v-else-if="message.reactions.reactionEnum">
            <div class="reaction-pill">
              <span class="reaction-emoji">{{ getReactionEmoji(message.reactions.reactionEnum) }}</span>
              <span class="reaction-count" v-if="Number(message.reactions.count) > 0">{{ message.reactions.count }}</span>
            </div>
          </template>
        </div>

        <div class="message-meta">
          <span>{{ message.time }}</span>
          <ReactionMessage :messageId="message.id" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import ReactionMessage from '@/components/ReactionMessage.vue'
import { getTelegramColor, getInitials } from '@/utils/colors' // Измените на '@/utils/color', если файл без 's'

const baseURL = import.meta.env.VITE_API_BASE_URL as string

export interface Message {
  id: string
  text: string
  time: string
  own?: boolean
  senderName?: string
  images?: string[]
  reactions?: { reactionEnum: number; count: number } | Array<{ reactionEnum: number; count: number }>
}

const getReactionEmoji = (enumVal: any): string => {
  const val = Number(enumVal)
  switch (val) {
    case 1: return '👍'
    case 2: return '❤️'
    case 3: return '🤡'
    case 4: return '💩'
    case 5: return '🔥'
    case 6: return '👎'
    default: return ''
  }
}

type GridClassName = '' | `grid-${1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9}`
type ItemClassName = 'natural' | 'square'

const props = defineProps<{ message: Message }>()

const imageCount = computed<number>(() => props.message.images?.length ?? 0)
const gridClass = computed<GridClassName>(() => {
  if (imageCount.value === 0) return ''
  return `grid-${imageCount.value}` as GridClassName
})

function itemClass(): ItemClassName {
  if (imageCount.value === 1) return 'natural'
  return 'square'
}
</script>

<style scoped>
.message-wrapper {
  display: flex;
  align-items: flex-end;
  gap: 8px;
  width: 100%;
}

.message-wrapper.own {
  justify-content: flex-end;
}

.message-wrapper.other {
  justify-content: flex-start;
}

.msg-avatar {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.75rem;
  font-weight: 700;
  flex-shrink: 0;
  user-select: none;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.message-bubble {
  --radius: 16px;
  --radius-inner: 8px;
  --gap: 3px;

  max-width: 85%;
  border-radius: var(--radius);
}

@media (min-width: 640px) {
  .message-bubble {
    max-width: 420px;
  }
}

.msg-author-tg {
  font-size: 0.8rem;
  font-weight: 600;
  padding: 6px 14px 0 14px;
  cursor: pointer;
  user-select: none;
}

.msg-author-tg:hover {
  text-decoration: underline;
}

.message-bubble.own {
  background: var(--bubble-own, #0f3460);
  border-bottom-right-radius: 4px;
}

.message-bubble.other {
  background: var(--bubble-other, #16213e);
  border-bottom-left-radius: 4px;
}

.message-content {
  margin: 4px 2px;
}

.message-text {
  padding: 6px 14px 8px;
  font-size: 0.935rem;
  line-height: 1.45;
  word-wrap: break-word;
}

.image-grid { display: grid; gap: var(--gap); }
.image-grid.grid-1 { grid-template-columns: 1fr; }
.image-grid.grid-2 { grid-template-columns: 1fr 1fr; }
.grid-item { position: relative; overflow: hidden; border-radius: var(--radius-inner); }
.grid-item img { display: block; width: 100%; height: 100%; object-fit: cover; }

.message-meta {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 6px;
  padding: 2px 12px 6px;
  font-size: 0.7rem;
  color: rgba(255, 255, 255, 0.5);
}

.reactions-container {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  padding: 4px 12px 6px;
}

.reaction-pill {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  background: rgba(255, 255, 255, 0.08);
  border-radius: 12px;
  padding: 2px 8px;
  font-size: 0.75rem;
}
</style>