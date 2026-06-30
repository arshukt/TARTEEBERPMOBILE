import api from "./api";
import type { ApiResponse, PagedResponse } from "@/types";

export interface Brand {
  id: number;
  brandName: string;
}

export interface CreateBrand {
  brandName: string;
}

export interface UpdateBrand extends CreateBrand {
  id: number;
}

export const brandService = {
  async getPaged(pageNumber: number, pageSize: number, searchTerm?: string): Promise<ApiResponse<PagedResponse<Brand>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      ...(searchTerm && { searchTerm })
    });
    const response = await api.get(`/brands?${params.toString()}`);
    return response.data;
  },

  async getAll(): Promise<ApiResponse<Brand[]>> {
    const response = await api.get(`/brands/all`);
    return response.data;
  },

  async getById(id: number): Promise<ApiResponse<Brand>> {
    const response = await api.get(`/brands/${id}`);
    return response.data;
  },

  async create(dto: CreateBrand): Promise<ApiResponse<Brand>> {
    const response = await api.post(`/brands`, dto);
    return response.data;
  },

  async update(dto: UpdateBrand): Promise<ApiResponse> {
    const response = await api.put(`/brands/${dto.id}`, dto);
    return response.data;
  },

  async delete(id: number): Promise<ApiResponse> {
    const response = await api.delete(`/brands/${id}`);
    return response.data;
  }
};
