

export interface ApiResponse<T = any> {
  success: boolean
  data?: T
  message: string
  errors?: string[]
}

export interface PagedResponse<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
}

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResponse {
  userId: number
  username: string
  fullName: string
  roleId: number
  roleName: string
  permissions: string[]
  accessToken: string
  refreshToken: string
  accessTokenExpiry: string
  refreshTokenExpiry: string
}

export interface RefreshTokenRequest {
  accessToken: string
  refreshToken: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

export interface User {
  id: number
  username: string
  fullName: string
  mobile?: string
  email?: string
  roleId: number
  roleName: string
  permissions?: string[]
  isActive: boolean
}

export interface SaleDto {
  date: string
  invoiceNumber: string
  customer: string
  total: number
}

export interface PurchaseDto {
  date: string
  invoiceNumber: string
  supplier: string
  total: number
}
