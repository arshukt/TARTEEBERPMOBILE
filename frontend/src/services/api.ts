import axios, { type AxiosRequestConfig, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/stores/auth'
import type { ApiResponse } from '@/types'

const api: ReturnType<typeof axios.create> = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const authStore = useAuthStore()
    if (authStore.token) {
      config.headers.Authorization = `Bearer ${authStore.token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

api.interceptors.response.use(
  (response: AxiosResponse<ApiResponse>) => {
    return response
  },
  async (error) => {
    const originalRequest = error.config
    const authStore = useAuthStore()

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      try {
        if (authStore.refreshToken) {
          const refreshResponse = await api.post<ApiResponse>('/auth/refresh-token', {
            accessToken: authStore.token,
            refreshToken: authStore.refreshToken
          })

          if (refreshResponse.data.success && refreshResponse.data.data) {
            authStore.setTokens(
              refreshResponse.data.data.accessToken,
              refreshResponse.data.data.refreshToken
            )
            originalRequest.headers.Authorization = `Bearer ${refreshResponse.data.data.accessToken}`
            return api(originalRequest)
          }
        }
      } catch (refreshError) {
        authStore.logout()
        window.location.href = '/login'
      }
    }

    return Promise.reject(error)
  }
)

export default api
