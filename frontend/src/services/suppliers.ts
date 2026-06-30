import api from './api'
import type { ApiResponse, PagedResponse } from '@/types'

export interface Supplier {
  id: number
  supplierCode: string
  supplierName: string
  mobile?: string
  email?: string
  address?: string
  openingBalance: number
}

export interface CreateSupplier {
  supplierCode: string
  supplierName: string
  mobile?: string
  email?: string
  address?: string
  openingBalance: number
}

export interface UpdateSupplier extends CreateSupplier {
  id: number
}

export const supplierService = {
  async getAll(): Promise<ApiResponse<Supplier[]>> {
    const response = await api.get(`/suppliers/all`)
    return response.data
  },

  async getPaged(pageNumber: number, pageSize: number, searchTerm?: string): Promise<ApiResponse<PagedResponse<Supplier>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      ...(searchTerm && { searchTerm })
    })
    const response = await api.get(`/suppliers?${params.toString()}`);
    return response.data
  },

  async getById(id: number): Promise<ApiResponse<Supplier>> {
    const response = await api.get(`/suppliers/${id}`)
    return response.data
  },

  async create(dto: CreateSupplier): Promise<ApiResponse<Supplier>> {
    const response = await api.post(`/suppliers`, dto)
    return response.data
  },

  async update(dto: UpdateSupplier): Promise<ApiResponse> {
    const response = await api.put(`/suppliers/${dto.id}`, dto)
    return response.data
  },

  async delete(id: number): Promise<ApiResponse> {
    const response = await api.delete(`/suppliers/${id}`)
    return response.data
  }
}
