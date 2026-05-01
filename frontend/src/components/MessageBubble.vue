<template>
  <div
    class="message-bubble"
    :class="[message.own ? 'own' : 'other', { 'has-images': imageCount > 0 }]"
  >
    <div class="message-content">
      <div class="message-text" v-if="message.text">{{ message.text }}</div>

      <div v-if="imageCount > 0" class="image-grid" :class="gridClass">
        <div v-for="(img, i) in message.images" :key="i" class="grid-item" :class="itemClass()">
          <img :src="`${baseURL}/${img}`" loading="lazy" alt="" />
        </div>
      </div>

      <div class="message-meta">
        <span>{{ message.time }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const baseURL = import.meta.env.VITE_API_BASE_URL as string

export interface Message {
  /** Текст сообщения (обязательно) */
  text: string
  /** Время отправки, например '12:05' */
  time: string
  /** true — своё сообщение (справа), false — чужое (слева) */
  own?: boolean
  /** Массив от 1 до 9 изображений */
  images?: string[]
}

type GridClassName = '' | `grid-${1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9}`
type ItemClassName = 'natural' | 'square'

const props = defineProps<{
  message: Message
}>()

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
.message-bubble {
  --radius: 16px;
  --radius-inner: 8px;
  --gap: 3px;

  max-width: 420px;
  border-radius: var(--radius);
}

.message-bubble.own {
  background: var(--bubble-own, #2d5af0);
  align-self: flex-end;
  border-bottom-right-radius: 4px;
}

.message-bubble.other {
  background: var(--bubble-other, #23232f);
  align-self: flex-start;
  border-bottom-left-radius: 4px;
}

.message-content {
  margin: 6px;
}

.message-text {
  padding: 10px 14px;
  font-size: 0.935rem;
  line-height: 1.45;
  word-wrap: break-word;
}

/* ===== Image Grid ===== */
.image-grid {
  display: grid;
  gap: var(--gap);
}

/* 1 — оригинальное соотношение */
.image-grid.grid-1 {
  grid-template-columns: 1fr;
}

/* 2 — два квадрата */
.image-grid.grid-2 {
  grid-template-columns: 1fr 1fr;
}

/* 3 — большое слева + 2 справа */
.image-grid.grid-3 {
  grid-template-columns: 2fr 1fr;
  grid-template-rows: 1fr 1fr;
}
.image-grid.grid-3 .grid-item:first-child {
  grid-row: 1 / 3;
}

/* 4 — сетка 2×2 */
.image-grid.grid-4 {
  grid-template-columns: 1fr 1fr;
  grid-template-rows: 1fr 1fr;
}

/* 5 — 2 вытянутых + 3 стандартных снизу */
.image-grid.grid-5 {
  grid-template-columns: repeat(6, 1fr);
  grid-template-rows: 1fr 1fr 1fr;
}
.image-grid.grid-5 .grid-item:nth-child(1) {
  grid-column: 1 / 4;
  grid-row: 1 / 3;
}
.image-grid.grid-5 .grid-item:nth-child(2) {
  grid-column: 4 / 7;
  grid-row: 1 / 3;
}
.image-grid.grid-5 .grid-item:nth-child(3) {
  grid-column: 1 / 3;
  grid-row: 3;
}
.image-grid.grid-5 .grid-item:nth-child(4) {
  grid-column: 3 / 5;
  grid-row: 3;
}
.image-grid.grid-5 .grid-item:nth-child(5) {
  grid-column: 5 / 7;
  grid-row: 3;
}

/* 6 — по 2 в строке, 3 ряда */
.image-grid.grid-6 {
  grid-template-columns: 1fr 1fr;
  grid-template-rows: 1fr 1fr 1fr;
}

/* 7 — 4 стандартных (2×2) + 3 маленьких (1×3) */
.image-grid.grid-7 {
  grid-template-columns: repeat(6, 1fr);
  grid-template-rows: 1fr 1fr auto;
}
.image-grid.grid-7 .grid-item:nth-child(1) {
  grid-column: 1 / 4;
  grid-row: 1;
}
.image-grid.grid-7 .grid-item:nth-child(2) {
  grid-column: 4 / 7;
  grid-row: 1;
}
.image-grid.grid-7 .grid-item:nth-child(3) {
  grid-column: 1 / 4;
  grid-row: 2;
}
.image-grid.grid-7 .grid-item:nth-child(4) {
  grid-column: 4 / 7;
  grid-row: 2;
}
.image-grid.grid-7 .grid-item:nth-child(5) {
  grid-column: 1 / 3;
  grid-row: 3;
}
.image-grid.grid-7 .grid-item:nth-child(6) {
  grid-column: 3 / 5;
  grid-row: 3;
}
.image-grid.grid-7 .grid-item:nth-child(7) {
  grid-column: 5 / 7;
  grid-row: 3;
}

/* 8 — 2 стандартных + 6 маленьких (2×3) */
.image-grid.grid-8 {
  grid-template-columns: repeat(6, 1fr);
  grid-template-rows: 1fr auto auto;
}
.image-grid.grid-8 .grid-item:nth-child(1) {
  grid-column: 1 / 4;
  grid-row: 1;
}
.image-grid.grid-8 .grid-item:nth-child(2) {
  grid-column: 4 / 7;
  grid-row: 1;
}
.image-grid.grid-8 .grid-item:nth-child(3) {
  grid-column: 1 / 3;
  grid-row: 2;
}
.image-grid.grid-8 .grid-item:nth-child(4) {
  grid-column: 3 / 5;
  grid-row: 2;
}
.image-grid.grid-8 .grid-item:nth-child(5) {
  grid-column: 5 / 7;
  grid-row: 2;
}
.image-grid.grid-8 .grid-item:nth-child(6) {
  grid-column: 1 / 3;
  grid-row: 3;
}
.image-grid.grid-8 .grid-item:nth-child(7) {
  grid-column: 3 / 5;
  grid-row: 3;
}
.image-grid.grid-8 .grid-item:nth-child(8) {
  grid-column: 5 / 7;
  grid-row: 3;
}

/* 9 — сетка 3×3 */
.image-grid.grid-9 {
  grid-template-columns: 1fr 1fr 1fr;
  grid-template-rows: 1fr 1fr 1fr;
}

/* ===== Grid Item ===== */
.grid-item {
  position: relative;
  overflow: hidden;
  border-radius: var(--radius-inner);
  background: rgba(0, 0, 0, 0.15);
}

.grid-item.square {
  aspect-ratio: 1;
}

.grid-item img {
  display: block;
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.grid-item.natural img {
  height: auto;
  object-fit: contain;
}

/* Meta */
.message-meta {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 4px;
  padding: 0 12px 8px;
  font-size: 0.7rem;
  color: rgba(255, 255, 255, 0.5);
}

.message-bubble.other .message-meta {
  color: rgba(255, 255, 255, 0.35);
}
</style>
