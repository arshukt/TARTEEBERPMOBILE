import api from "./api";
import type { ApiResponse, PagedResponse } from "@/types";

export interface Item {
  id: number;
  barcode?: string;
  itemCode: string;
  itemName: string;
  categoryId: number;
  brandId: number;
  unitId: number;
  purchaseRate: number;
  costRate: number;
  wholesaleRate: number;
  retailRate: number;
  mrp: number;
  taxPercentage: number;
  minimumStock: number;
  openingStock: number;
  currentStock?: number;
  isActive: boolean;
  itemImage?: string;
  categoryName?: string;
  brandName?: string;
  unitName?: string;
}

export interface CreateItem {
  barcode?: string;
  itemCode: string;
  itemName: string;
  categoryId: number;
  brandId: number;
  unitId: number;
  purchaseRate: number;
  costRate: number;
  wholesaleRate: number;
  retailRate: number;
  mrp: number;
  taxPercentage: number;
  minimumStock: number;
  openingStock: number;
  isActive: boolean;
  itemImage?: string;
}

export interface UpdateItem extends CreateItem {
  id: number;
}

export const itemService = {
  async getPaged(pageNumber: number, pageSize: number, searchTerm?: string): Promise<ApiResponse<PagedResponse<Item>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      ...(searchTerm && { searchTerm })
    });
    const response = await api.get(`/items?${params.toString()}`);
    return response.data;
  },

  async getAll(): Promise<ApiResponse<Item[]>> {
    const response = await api.get(`/items/all`);
    return response.data;
  },

  async getById(id: number): Promise<ApiResponse<Item>> {
    const response = await api.get(`/items/${id}`);
    return response.data;
  },

  async create(dto: CreateItem): Promise<ApiResponse<Item>> {
    const response = await api.post(`/items`, dto);
    return response.data;
  },

  async update(dto: UpdateItem): Promise<ApiResponse> {
    const response = await api.put(`/items/${dto.id}`, dto);
    return response.data;
  },

  async delete(id: number): Promise<ApiResponse> {
    const response = await api.delete(`/items/${id}`);
    return response.data;
  }
};
