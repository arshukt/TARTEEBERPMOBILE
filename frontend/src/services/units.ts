import api from "./api";
import type { ApiResponse, PagedResponse } from "@/types";

export interface Unit {
  id: number;
  unitName: string;
  symbol: string;
}

export interface CreateUnit {
  unitName: string;
  symbol: string;
}

export interface UpdateUnit extends CreateUnit {
  id: number;
}

export const unitService = {
  async getPaged(pageNumber: number, pageSize: number, searchTerm?: string): Promise<ApiResponse<PagedResponse<Unit>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      ...(searchTerm && { searchTerm })
    });
    const response = await api.get(`/units?${params.toString()}`);
    return response.data;
  },

  async getAll(): Promise<ApiResponse<Unit[]>> {
    const response = await api.get(`/units/all`);
    return response.data;
  },

  async getById(id: number): Promise<ApiResponse<Unit>> {
    const response = await api.get(`/units/${id}`);
    return response.data;
  },

  async create(dto: CreateUnit): Promise<ApiResponse<Unit>> {
    const response = await api.post(`/units`, dto);
    return response.data;
  },

  async update(dto: UpdateUnit): Promise<ApiResponse> {
    const response = await api.put(`/units/${dto.id}`, dto);
    return response.data;
  },

  async delete(id: number): Promise<ApiResponse> {
    const response = await api.delete(`/units/${id}`);
    return response.data;
  }
};
