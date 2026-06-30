import api from "./api";
import type { ApiResponse, PagedResponse } from "@/types";
import type { Item } from "./items";
import type { Customer } from "./customers";

export interface SaleDetail {
    id: number;
    itemId: number;
    quantity: number;
    rate: number;
    discount: number;
    taxPercentage: number;
    taxAmount: number;
    total: number;
    item?: Item;
}

export interface CreateSaleDetail {
    itemId: number;
    quantity: number;
    rate: number;
    discount: number;
    taxPercentage: number;
}

export interface Sale {
    id: number;
    customerId?: number;
    saleDate: string;
    invoiceNumber: string;
    totalAmount: number;
    discount: number;
    taxAmount: number;
    netAmount: number;
    paidAmount: number;
    dueAmount: number;
    dueDate?: string;
    isCredit: boolean;
    saleDetails: SaleDetail[];
    customer?: Customer;
}

export interface CreateSale {
    customerId?: number;
    saleDate: string;
    invoiceNumber: string;
    saleDetails: CreateSaleDetail[];
    paidAmount: number;
    dueDate?: string;
    isCredit: boolean;
}

export interface UpdateSale extends CreateSale {
    id: number;
}

export const saleService = {
    async getPaged(pageNumber: number, pageSize: number, searchTerm?: string): Promise<ApiResponse<PagedResponse<Sale>>> {
        const params = new URLSearchParams({
            pageNumber: pageNumber.toString(),
            pageSize: pageSize.toString(),
            ...(searchTerm && { searchTerm })
        });
        const response = await api.get(`/sales?${params.toString()}`);
        return response.data;
    },

    async getById(id: number): Promise<ApiResponse<Sale>> {
        const response = await api.get(`/sales/${id}`);
        return response.data;
    },

    async create(dto: CreateSale): Promise<ApiResponse<Sale>> {
        const response = await api.post(`/sales`, dto);
        return response.data;
    },

    async update(dto: UpdateSale): Promise<ApiResponse> {
        const response = await api.put(`/sales/${dto.id}`, dto);
        return response.data;
    },

    async delete(id: number): Promise<ApiResponse> {
        const response = await api.delete(`/sales/${id}`);
        return response.data;
    }
};
