import api from './api'
import type { ApiResponse, LoginRequest, LoginResponse, RefreshTokenRequest, ChangePasswordRequest, User } from '@/types'

export const authService = {
  async login(credentials: LoginRequest): Promise<ApiResponse<LoginResponse>> {
    const response = await api.post<ApiResponse<LoginResponse>>('/auth/login', credentials)
    return response.data
  },

  async refreshToken(request: RefreshTokenRequest): Promise<ApiResponse<LoginResponse>> {
    const response = await api.post<ApiResponse<LoginResponse>>('/auth/refresh-token', request)
    return response.data
  },

  async getCurrentUser(): Promise<ApiResponse<User>> {
    const response = await api.get<ApiResponse<User>>('/auth/me')
    return response.data
  },

  async changePassword(request: ChangePasswordRequest): Promise<ApiResponse> {
    const response = await api.post<ApiResponse>('/auth/change-password', request)
    return response.data
  }
}
