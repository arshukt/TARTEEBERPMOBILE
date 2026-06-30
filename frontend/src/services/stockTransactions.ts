import api from "./api";
import type { ApiResponse, PagedResponse } from "@/types";

export interface StockTransaction {
  id: number;
  itemId: number;
  itemName?: string;
  transactionType: number;
  transactionTypeName?: string;
  quantityIn: number;
  quantityOut: number;
  balanceAfter: number;
  referenceId: number;
  referenceType: string;
  notes?: string;
  transactionDate: string;
}

export const stockTransactionService = {
  async getPaged(pageNumber: number, pageSize: number, searchTerm?: string, itemId?: number): Promise<ApiResponse<PagedResponse<StockTransaction>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      ...(searchTerm && { searchTerm }),
      ...(itemId && { itemId: itemId.toString() })
    });
    const response = await api.get(`/stocktransactions?${params.toString()}`);
    return response.data;
  }
};
