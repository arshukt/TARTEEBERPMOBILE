import { ref, watch } from 'vue'

const isDark = ref(localStorage.getItem('theme-dark') === 'true')

const applyTheme = (dark: boolean) => {
  if (dark) {
    document.documentElement.classList.add('dark')
  } else {
    document.documentElement.classList.remove('dark')
  }
}

// Initial application
applyTheme(isDark.value)

watch(isDark, (newValue) => {
  localStorage.setItem('theme-dark', newValue.toString())
  applyTheme(newValue)
})

export function useTheme() {
  const toggleTheme = () => {
    isDark.value = !isDark.value
  }

  return {
    isDark,
    toggleTheme
  }
}
