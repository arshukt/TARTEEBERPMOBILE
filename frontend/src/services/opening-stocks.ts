import api from "./api";
import type { ApiResponse, PagedResponse } from "@/types";
import type { Item } from "./items";

export interface OpeningStockDetail {
    id: number;
    itemId: number;
    purchaseRate: number;
    costRate: number;
    retailRate: number;
    wholesaleRate: number;
    mrp: number;
    quantity: number;
    batchNumber?: string;
    expiryDate?: string;
    item?: Item;
}

export interface CreateOpeningStockDetail {
    itemId: number;
    purchaseRate: number;
    costRate: number;
    retailRate: number;
    wholesaleRate: number;
    mrp: number;
    quantity: number;
    batchNumber?: string;
    expiryDate?: string;
}

export interface OpeningStock {
    id: number;
    date: string;
    notes?: string;
    openingStockDetails: OpeningStockDetail[];
}

export interface CreateOpeningStock {
    date: string;
    notes?: string;
    openingStockDetails: CreateOpeningStockDetail[];
}

export interface UpdateOpeningStock extends CreateOpeningStock {
    id: number;
}

export const openingStockService = {
    async getPaged(pageNumber: number, pageSize: number, searchTerm?: string): Promise<ApiResponse<PagedResponse<OpeningStock>>> {
        const params = new URLSearchParams({
            pageNumber: pageNumber.toString(),
            pageSize: pageSize.toString(),
            ...(searchTerm && { searchTerm })
        });
        const response = await api.get(`/openingstocks?${params.toString()}`);
        return response.data;
    },

    async getById(id: number): Promise<ApiResponse<OpeningStock>> {
        const response = await api.get(`/openingstocks/${id}`);
        return response.data;
    },

    async create(dto: CreateOpeningStock): Promise<ApiResponse<OpeningStock>> {
        const response = await api.post(`/openingstocks`, dto);
        return response.data;
    },

    async update(dto: UpdateOpeningStock): Promise<ApiResponse> {
        const response = await api.put(`/openingstocks/${dto.id}`, dto);
        return response.data;
    },

    async delete(id: number): Promise<ApiResponse> {
        const response = await api.delete(`/openingstocks/${id}`);
        return response.data;
    }
};
