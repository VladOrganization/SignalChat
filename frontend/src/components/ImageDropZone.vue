<template>
  <div
    class="drop-zone"
    :class="{ dragover: isDragging }"
    @dragenter.prevent="onDragEnter"
    @dragover.prevent
    @dragleave="onDragLeave"
    @drop.prevent="onDrop"
  >
    <slot>
      <svg class="drop-icon" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
        <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
        <polyline points="17 8 12 3 7 8" />
        <line x1="12" y1="3" x2="12" y2="15" />
      </svg>
      <p class="drop-text">Перетащите изображения сюда</p>
    </slot>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const emit = defineEmits(['drop'])

const isDragging = ref(false)
let dragCounter = 0

function onDragEnter() {
  dragCounter++
  isDragging.value = true
}

function onDragLeave() {
  dragCounter--
  if (dragCounter === 0) {
    isDragging.value = false
  }
}

function onDrop(e: DragEvent) {
  dragCounter = 0
  isDragging.value = false

  const files = Array.from(e.dataTransfer?.files || []).filter((f) =>
    f.type.startsWith('image/')
  )

  if (files.length) {
    emit('drop', files)
  }
}
</script>

<style scoped>
.drop-zone {
  border: 2px dashed #ccc;
  border-radius: 12px;
  padding: 2rem;
  text-align: center;
  transition: border-color 0.2s, background 0.2s;
}

.drop-zone.dragover {
  border-color: #5b9bd5;
  background: #eef5fc;
}

.drop-icon {
  color: #bbb;
  margin-bottom: 8px;
}

.drop-text {
  margin: 0;
  color: #999;
  font-size: 14px;
}
</style>
