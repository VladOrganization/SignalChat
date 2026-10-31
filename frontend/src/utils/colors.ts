/**
 * Генерирует детерминированный цвет имени отправителя в стиле Telegram.
 */
export function getTelegramColor(name?: string | null): string {
  const safeName = name ?? ''
  
  const colors: string[] = [
    '#e17076', // Красный
    '#7bc862', // Зеленый
    '#e67e22', // Оранжевый
    '#2196f3', // Синий
    '#9c27b0', // Фиолетовый
    '#00bcd4', // Голубой
    '#3f51b5', // Индиго
    '#e91e63', // Розовый
  ]

  if (!safeName) {
    return colors[0]!
  }

  let hash = 0
  for (let i = 0; i < safeName.length; i++) {
    hash = safeName.charCodeAt(i) + ((hash << 5) - hash)
  }

  const index = Math.abs(hash) % colors.length
  return colors[index] ?? colors[0]!
}

/**
 * Получает первые 1-2 буквы из имени пользователя (инициалы)
 */
export function getInitials(name?: string | null): string {
  if (!name) return 'U'
  const trimmed = name.trim()
  if (!trimmed) return 'U'

  const parts = trimmed.split(/\s+/)
  if (parts.length >= 2 && parts[0]?.[0] && parts[1]?.[0]) {
    return (parts[0][0] + parts[1][0]).toUpperCase()
  }

  return trimmed.slice(0, 2).toUpperCase()
}