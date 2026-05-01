<template>
  <button class="upload-btn" @click="openDialog" :disabled>
    <i class="fa-solid fa-paperclip"></i>
  </button>
  <input
    ref="fileInputRef"
    type="file"
    multiple
    accept="image/*"
    class="hidden-input"
    @change="onFileChange"
  />
</template>

<script setup lang="ts">
import { ref } from 'vue'

const emit = defineEmits(['select'])

defineProps<{ disabled?: boolean }>()

const fileInputRef = ref<HTMLInputElement>()

function openDialog() {
  fileInputRef.value?.click()
}

function onFileChange(e: Event) {
  const target = e.target as HTMLInputElement
  const files = Array.from(target.files || [])
  if (files.length) {
    emit('select', files)
  }
  // Сброс value, чтобы можно было повторно выбрать тот же файл
  target.value = ''
}
</script>

<style scoped>
.upload-btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  background: #6c63ff;
  color: #fff;
  font-size: 14px;
  cursor: pointer;
  transition:
    background 0.2s,
    border-color 0.2s;
}

.upload-btn:hover {
  background: #7c73ff;
}

.upload-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.upload-btn:active {
  transform: scale(0.98);
}

.hidden-input {
  display: none;
}
</style>
