import api from "./api";
import type { ApiResponse, PagedResponse } from "@/types";
import type { Item } from "./items";
import type { Supplier } from "./suppliers";

export interface PurchaseDetail {
    id: number;
    itemId: number;
    quantity: number;
    purchaseRate: number;
    costRate: number;
    retailRate: number;
    wholesaleRate: number;
    mrp: number;
    discount: number;
    taxPercentage: number;
    taxAmount: number;
    total: number;
    batchNumber?: string;
    expiryDate?: string;
    item?: Item;
}

export interface CreatePurchaseDetail {
    itemId: number;
    quantity: number;
    purchaseRate: number;
    costRate: number;
    retailRate: number;
    wholesaleRate: number;
    mrp: number;
    discount: number;
    taxPercentage: number;
    batchNumber?: string;
    expiryDate?: string;
}

export interface Purchase {
    id: number;
    supplierId: number;
    purchaseDate: string;
    invoiceNumber: string;
    totalAmount: number;
    discount: number;
    taxAmount: number;
    netAmount: number;
    purchaseDetails: PurchaseDetail[];
    supplier?: Supplier;
}

export interface CreatePurchase {
    supplierId: number;
    purchaseDate: string;
    invoiceNumber: string;
    purchaseDetails: CreatePurchaseDetail[];
}

export interface UpdatePurchase extends CreatePurchase {
    id: number;
}

export const purchaseService = {
    async getPaged(pageNumber: number, pageSize: number, searchTerm?: string): Promise<ApiResponse<PagedResponse<Purchase>>> {
        const params = new URLSearchParams({
            pageNumber: pageNumber.toString(),
            pageSize: pageSize.toString(),
            ...(searchTerm && { searchTerm })
        });
        const response = await api.get(`/purchases?${params.toString()}`);
        return response.data;
    },

    async getById(id: number): Promise<ApiResponse<Purchase>> {
        const response = await api.get(`/purchases/${id}`);
        return response.data;
    },

    async create(dto: CreatePurchase): Promise<ApiResponse<Purchase>> {
        const response = await api.post(`/purchases`, dto);
        return response.data;
    },

    async update(dto: UpdatePurchase): Promise<ApiResponse> {
        const response = await api.put(`/purchases/${dto.id}`, dto);
        return response.data;
    },

    async delete(id: number): Promise<ApiResponse> {
        const response = await api.delete(`/purchases/${id}`);
        return response.data;
    }
};
