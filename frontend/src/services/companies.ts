import api from "./api";
import type { ApiResponse, PagedResponse } from "@/types";

export interface Company {
  id: number;
  companyName: string;
  address?: string;
  mobile?: string;
  phone?: string;
  email?: string;
  website?: string;
  logo?: string;
  taxNumber?: string;
}

export interface CreateCompany {
  companyName: string;
  address?: string;
  mobile?: string;
  phone?: string;
  email?: string;
  website?: string;
  logo?: string;
  taxNumber?: string;
}

export interface UpdateCompany extends CreateCompany {
  id: number;
}

export const companyService = {
  async getPaged(pageNumber: number, pageSize: number, searchTerm?: string): Promise<ApiResponse<PagedResponse<Company>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      ...(searchTerm && { searchTerm })
    });
    const response = await api.get(`/companies?${params.toString()}`);
    return response.data;
  },

  async getAll(): Promise<ApiResponse<Company[]>> {
    const response = await api.get(`/companies/all`);
    return response.data;
  },

  async getFirst(): Promise<ApiResponse<Company | null>> {
    const response = await api.get(`/companies/first`);
    return response.data;
  },

  async getById(id: number): Promise<ApiResponse<Company>> {
    const response = await api.get(`/companies/${id}`);
    return response.data;
  },

  async create(dto: CreateCompany): Promise<ApiResponse<Company>> {
    const response = await api.post(`/companies`, dto);
    return response.data;
  },

  async update(dto: UpdateCompany): Promise<ApiResponse> {
    const response = await api.put(`/companies/${dto.id}`, dto);
    return response.data;
  },

  async delete(id: number): Promise<ApiResponse> {
    const response = await api.delete(`/companies/${id}`);
    return response.data;
  }
};