import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authService } from '@/services/auth'
import type { LoginRequest, LoginResponse, User } from '@/types'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('accessToken'))
  const refreshToken = ref<string | null>(localStorage.getItem('refreshToken'))
  const user = ref<User | null>(null)
  const loading = ref(false)

  const isAuthenticated = computed(() => !!token.value)

  const setTokens = (accessToken: string, newRefreshToken: string) => {
    token.value = accessToken
    refreshToken.value = newRefreshToken
    localStorage.setItem('accessToken', accessToken)
    localStorage.setItem('refreshToken', newRefreshToken)
  }

  const login = async (credentials: LoginRequest) => {
    loading.value = true
    try {
      const response = await authService.login(credentials)
      if (response.success && response.data) {
        setTokens(response.data.accessToken, response.data.refreshToken)
        user.value = {
          id: response.data.userId,
          username: response.data.username,
          fullName: response.data.fullName,
          roleId: response.data.roleId,
          roleName: response.data.roleName,
          permissions: response.data.permissions,
          isActive: true
        }
        return true
      }
      return false
    } finally {
      loading.value = false
    }
  }

  const fetchCurrentUser = async () => {
    try {
      const response = await authService.getCurrentUser()
      if (response.success && response.data) {
        user.value = response.data
      }
    } catch (error) {
      console.error('Failed to fetch current user:', error)
    }
  }

  const logout = () => {
    token.value = null
    refreshToken.value = null
    user.value = null
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
  }

  return {
    token,
    refreshToken,
    user,
    loading,
    isAuthenticated,
    setTokens,
    login,
    fetchCurrentUser,
    logout
  }
})
