import api from "./api";
import type { ApiResponse, PagedResponse } from "@/types";

export interface Customer {
  id: number;
  customerCode: string;
  customerName: string;
  contactPerson?: string;
  mobile?: string;
  whatsApp?: string;
  email?: string;
  address?: string;
  city?: string;
  creditDays: number;
  creditLimit: number;
  openingBalance: number;
  isActive: boolean;
}

export interface CreateCustomer {
  customerCode: string;
  customerName: string;
  contactPerson?: string;
  mobile?: string;
  whatsApp?: string;
  email?: string;
  address?: string;
  city?: string;
  creditDays: number;
  creditLimit: number;
  openingBalance: number;
  isActive: boolean;
}

export interface UpdateCustomer extends CreateCustomer {
  id: number;
}

export const customerService = {
  async getAll(): Promise<ApiResponse<Customer[]>> {
    const response = await api.get(`/customers/all`);
    return response.data;
  },

  async getPaged(pageNumber: number, pageSize: number, searchTerm?: string): Promise<ApiResponse<PagedResponse<Customer>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      ...(searchTerm && { searchTerm })
    });
    const response = await api.get(`/customers?${params.toString()}`);
    return response.data;
  },

  async getById(id: number): Promise<ApiResponse<Customer>> {
    const response = await api.get(`/customers/${id}`);
    return response.data;
  },

  async create(dto: CreateCustomer): Promise<ApiResponse<Customer>> {
    const response = await api.post(`/customers`, dto);
    return response.data;
  },

  async update(dto: UpdateCustomer): Promise<ApiResponse> {
    const response = await api.put(`/customers/${dto.id}`, dto);
    return response.data;
  },

  async delete(id: number): Promise<ApiResponse> {
    const response = await api.delete(`/customers/${id}`);
    return response.data;
  }
};
