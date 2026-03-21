<template>
  <div v-if="images.length" class="preview-grid">
    <div v-for="(img, index) in images" :key="img.id" class="preview-item">
      <img :src="img.url" :alt="img.name" />
      <button class="remove-btn" @click="$emit('remove', index)" aria-label="Удалить">
        <svg
          width="14"
          height="14"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          stroke-linecap="round"
        >
          <line x1="18" y1="6" x2="6" y2="18" />
          <line x1="6" y1="6" x2="18" y2="18" />
        </svg>
      </button>
      <div class="file-name">{{ img.name }}</div>
    </div>
  </div>
  <hr v-if="images.length" />
</template>

<script setup lang="ts">
import type { ImageFile } from '@/components/ImageUploaderExample.vue'

defineProps({
  images: {
    type: Array<ImageFile>,
    required: true,
  },
})

defineEmits(['remove'])
</script>

<style scoped>
.preview-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
  gap: 12px;
  margin-bottom: 6px;
}

hr {
  margin-bottom: 6px;
  color: #2a2a4a;
}

.preview-item {
  position: relative;
  aspect-ratio: 1;
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid #e0e0e0;
  background: #fafafa;
}

.preview-item img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.remove-btn {
  position: absolute;
  top: 6px;
  right: 6px;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: rgba(0, 0, 0, 0.55);
  color: #fff;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition:
    opacity 0.2s,
    background 0.2s;
}

.preview-item:hover .remove-btn {
  opacity: 1;
}

.remove-btn:hover {
  background: rgba(0, 0, 0, 0.8);
}

.file-name {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  background: rgba(0, 0, 0, 0.5);
  color: #fff;
  font-size: 11px;
  padding: 3px 6px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
