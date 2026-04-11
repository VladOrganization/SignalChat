<template>
  <div class="telegram-dropdown">
    <!-- Кнопка с плюсом -->
    <button class="plus-button" @click="toggleDropdown">+</button>

    <!-- Выпадающий список -->
    <div v-if="isOpen" class="dropdown-menu">
      <div
        v-for="option in options"
        :key="option.value"
        class="dropdown-item"
        @click="selectOption(option)"
      >
        {{ option.text }}
      </div>
    </div>

    <!-- Отображение выбранного значения -->
    <p v-if="selectedValue" class="selected-text">
      Выбрано: {{ selectedValue }}
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const isOpen = ref(false)
const selectedValue = ref('apple')


function toggleDropdown() {
  isOpen.value = !isOpen.value
}

async function selectOption(option:Number) {
  selectedValue.value = option.value
  isOpen.value = false
}


const options = [
  {id:1, text: '👍', value: '👍' },
  {id:2, text: '❤️', value: '❤️' },
  {id:3, text: '🤡', value: '🤡' },
  {id:4, text: '💩', value: '💩' },
  {id:5, text: '🔥', value: '🔥' },
  {id:6, text: '👎', value: '👎' },

]
</script>


<style scoped>
.telegram-dropdown {
  position: relative;
  display: inline-block;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
}

/* Кнопка с плюсом в стиле Telegram */
.plus-button {
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background-color: #3390ec; /* Telegram blue */
  border: none;
  color: white;
  font-size: 18px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: opacity 0.2s ease;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
}

.plus-button:hover {
  opacity: 0.85;
}

.plus-button:active {
  transform: scale(0.96);
}

/* Выпадающее меню — как в Telegram */
/* Выпадающее меню — появляется СВЕРХУ */
.dropdown-menu {
  position: absolute;
  bottom: 100%;        /* привязываем к нижнему краю кнопки */
  left: 0;
  margin-bottom: 8px;  /* отступ от кнопки */
  min-width: 50px;
  background-color: #ffffff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  overflow: hidden;
  z-index: 1000;
  padding: 6px 0;
}

/* Пункты списка */
.dropdown-item {
  padding: 12px 16px;
  font-size: 16px;
  color: #000000;
  cursor: pointer;
  transition: background-color 0.15s;
}

.dropdown-item:hover {
  background-color: #f0f2f5;
}

/* Отображение выбранного значения */
.selected-text {
  margin-top: 12px;
  font-size: 14px;
  color: #5e6670;
}

/* Тёмная тема Telegram */
@media (prefers-color-scheme: dark) {
  .dropdown-menu {
    background-color: #1c1c1e;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
  }
  .dropdown-item {
    color: #ffffff;
  }
  .dropdown-item:hover {
    background-color: #2c2c2e;
  }
  .selected-text {
    color: #8e9eae;
  }
}
</style>
