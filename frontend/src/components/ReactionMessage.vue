<template>
  <div class="reaction-picker">
            <button class="plus-btn" @click="showMenu = !showMenu">+</button>
            <div v-if="showMenu" class="reactions-list">
                <button
                    v-for="r in reactions"
                    :key="r.id"
                    class="reaction"
                    @click="selectReaction(r)"
                >
                    {{ r.emoji }}
                </button>
            </div>
        </div>
</template>

<style scoped>
        * {
            box-sizing: border-box;
        }

        /* контейнер для компонента */
        .reaction-picker {
            position: relative;
            display: inline-block;
        }
        /* кнопка "+" */
        .plus-btn {
            width: 25px;
            height: 25px;
            border-radius: 50%;
            background: white;
            border: none;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            font-size: 18px;
            font-weight: 300;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: 0.1s;
        }
        .plus-btn:active {
            transform: scale(0.96);
        }
        /* список реакций */
        .reactions-list {
            position: absolute;
            bottom: 70px;
            left: 70%;
            transform: translateX(-50%);
            background: white;
            border-radius: 48px;
            padding: 8px 16px;
            display: flex;
            flex-direction: column;
            gap: 12px;
            box-shadow: 0 10px 25px rgba(0,0,0,0.2);
            white-space: nowrap;
            z-index: 10;
        }
        .reaction {
            background: none;
            border: none;
            font-size: 28px;
            cursor: pointer;
            padding: 6px 8px;
            border-radius: 40px;
            transition: 0.1s;
        }
        .reaction:hover {
            background: #e9ecef;
            transform: scale(1.1);
        }
        .feedback {
            margin-top: 30px;
            text-align: center;
            background: #2c3e50;
            color: white;
            padding: 8px 20px;
            border-radius: 40px;
            font-size: 14px;
        }
</style>

<script setup lang="ts">
  import { ref } from 'vue';

  const reactions = ref([
                    { id: 1, emoji: '👍', name: 'Нравиться' },
                    { id: 2, emoji: '❤️', name: 'Сердце' },
                    { id: 3, emoji: '🤡', name: 'Клоун' },
                    { id: 4, emoji: '💩', name: 'Дерьмо' },
                    { id: 5, emoji: '🔥', name: 'Огонь' },
                    { id: 6, emoji: '👎', name: 'Не нравиться' }
                ]);

                const showMenu = ref(false);
                const lastReaction = ref(null);

                const selectReaction = (reaction) => {
                    lastReaction.value = reaction;
                    console.log(lastReaction.value);
                    showMenu.value = false;   // закрыть меню после выбора
                };

</script>
