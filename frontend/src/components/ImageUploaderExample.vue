<template>
  <div class="image-uploader">
    <ImageDropZone @drop="addFiles" />

    <div class="actions">
      <ImageUploadButton @select="addFiles" />
      <span v-if="images.length" class="count">{{ images.length }} файл(ов)</span>
    </div>

    <ImagePreviewGrid :images="images" @remove="removeFile" />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import ImageDropZone from '@/components/ImageDropZone.vue'
import ImageUploadButton from '@/components/ImageUploadButton.vue'
import ImagePreviewGrid from '@/components/ImagePreviewGrid.vue'
import * as trace_events from 'node:trace_events'

const images = ref<ImageFile[]>([])
let idCounter = 0

export interface ImageFile {
  id: string
  name: string
  url: string
  file: File
}

function addFiles(files: File[]) {
  for (const file of files) {
    const url = URL.createObjectURL(file)
    images.value.push({
      id: String(idCounter++),
      name: file.name,
      url,
      file, // оригинальный File-объект, если нужен для отправки на сервер
    })
  }
}

function removeFile(index: number) {
  const [removed] = images.value.splice(index, 1)

  if (!removed) {
    return
  }

  URL.revokeObjectURL(removed.url)
}
</script>

<style scoped>
.image-uploader {
  display: flex;
  flex-direction: column;
  gap: 16px;
  max-width: 600px;
}

.actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

.count {
  font-size: 13px;
  color: #888;
}
</style>
