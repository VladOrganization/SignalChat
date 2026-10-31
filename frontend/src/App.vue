<template>
  <div class="app-container">
    <AuthView v-if="!auth.isAuthenticated" @authenticated="onAuthenticated" />
    <ChatView v-else @logout="onLogout" />
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import AuthView from '@/components/AuthView.vue'
import ChatView from '@/components/ChatView.vue'

const auth = useAuthStore()
function onAuthenticated() {}
function onLogout() {}
</script>

<style>
*,
*::before,
*::after {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

html, body, #app {
  width: 100%;
  height: 100%;
  height: 100dvh; /* Корректная высота под мобильные браузеры */
  overflow: hidden;
  background: #1a1a2e;
}

body {
  font-family: system-ui, -apple-system, sans-serif;
  color: #e0e0ff;
}

.app-container {
  width: 100%;
  height: 100%;
}

::-webkit-scrollbar {
  width: 5px;
}

::-webkit-scrollbar-track {
  background: transparent;
}

::-webkit-scrollbar-thumb {
  background: #2a2a4a;
  border-radius: 3px;
}
</style>