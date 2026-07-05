import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import App from './App.vue'
import '@fortawesome/fontawesome-free/css/all.min.css'
import { createRouter, createWebHistory } from 'vue-router'
import AuthView from './components/AuthView.vue'

const routes = [
  {
    path: '/',
    name: 'auth',
    component: AuthView
  },
  {
    path: '/global-chat',
    name: 'about',
    // Lazy-loaded route for better performance
    component: () => import('../src/components/ChatView.vue')
  },
]
export const router = createRouter({
  history: createWebHistory(),
  routes,
})

const app = createApp(App)

const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)
app.use(router);
app.use(pinia)
app.mount('#app')
