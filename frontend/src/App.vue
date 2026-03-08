<template>
  <AuthView v-if="!auth.isAuthenticated" @authenticated="onAuthenticated" />
  <ChatView v-else @logout="onLogout" />
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

body {
  font-family: system-ui, -apple-system, sans-serif;
  background: #1a1a2e;
}

::-webkit-scrollbar {
  width: 6px;
}

::-webkit-scrollbar-track {
  background: transparent;
}

::-webkit-scrollbar-thumb {
  background: #2a2a4a;
  border-radius: 3px;
}
</style>
