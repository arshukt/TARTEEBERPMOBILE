import api from "./api";
import type { ApiResponse, PagedResponse } from "@/types";

export interface Category {
  id: number;
  categoryName: string;
  description?: string;
}

export interface CreateCategory {
  categoryName: string;
  description?: string;
}

export interface UpdateCategory extends CreateCategory {
  id: number;
}

export const categoryService = {
  async getPaged(pageNumber: number, pageSize: number, searchTerm?: string): Promise<ApiResponse<PagedResponse<Category>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      ...(searchTerm && { searchTerm })
    });
    const response = await api.get(`/categories?${params.toString()}`);
    return response.data;
  },

  async getAll(): Promise<ApiResponse<Category[]>> {
    const response = await api.get(`/categories/all`);
    return response.data;
  },

  async getById(id: number): Promise<ApiResponse<Category>> {
    const response = await api.get(`/categories/${id}`);
    return response.data;
  },

  async create(dto: CreateCategory): Promise<ApiResponse<Category>> {
    const response = await api.post(`/categories`, dto);
    return response.data;
  },

  async update(dto: UpdateCategory): Promise<ApiResponse> {
    const response = await api.put(`/categories/${dto.id}`, dto);
    return response.data;
  },

  async delete(id: number): Promise<ApiResponse> {
    const response = await api.delete(`/categories/${id}`);
    return response.data;
  }
};
