import api from './api'
import type { ApiResponse } from '@/types'

export interface CurrentStockReportItem {
  id: number
  code: string
  name: string
  category: string
  currentStock: number
  unit: string
}

export interface SalesReportItem {
  date: string
  invoiceNumber: string
  customer: string
  total: number
}

export interface PurchasesReportItem {
  date: string
  invoiceNumber: string
  supplier: string
  total: number
}

export interface CustomerOutstandingReportItem {
  customerId: number
  customerName: string
  totalOutstanding: number
  lastTransaction?: string
}

export interface SupplierOutstandingReportItem {
  supplierId: number
  supplierName: string
  totalOutstanding: number
  lastTransaction?: string
}

export const reportService = {
  async getCurrentStock(): Promise<ApiResponse<CurrentStockReportItem[]>> {
    const response = await api.get('/reports/current-stock')
    return response.data
  },

  async getSales(startDate?: string, endDate?: string): Promise<ApiResponse<SalesReportItem[]>> {
    const params = new URLSearchParams()
    if (startDate) params.append('startDate', startDate)
    if (endDate) params.append('endDate', endDate)
    const response = await api.get(`/reports/sales?${params.toString()}`)
    return response.data
  },

  async getPurchases(startDate?: string, endDate?: string): Promise<ApiResponse<PurchasesReportItem[]>> {
    const params = new URLSearchParams()
    if (startDate) params.append('startDate', startDate)
    if (endDate) params.append('endDate', endDate)
    const response = await api.get(`/reports/purchases?${params.toString()}`)
    return response.data
  },

  async getCustomerOutstanding(): Promise<ApiResponse<CustomerOutstandingReportItem[]>> {
    const response = await api.get('/reports/customer-outstanding')
    return response.data
  },

  async getSupplierOutstanding(): Promise<ApiResponse<SupplierOutstandingReportItem[]>> {
    const response = await api.get('/reports/supplier-outstanding')
    return response.data
  }
}
