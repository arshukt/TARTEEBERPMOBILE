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

export interface PurchaseReturn {
  id: number;
  purchaseId: number;
  returnDate: string;
  returnNumber: string;
  totalAmount: number;
  discount: number;
  taxAmount: number;
  netAmount: number;
  notes?: string;
  purchaseReturnDetails: PurchaseReturnDetail[];
}

export interface PurchaseReturnDetail {
  id: number;
  purchaseReturnId: number;
  purchaseDetailId: number;
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
  reason?: string;
}

export interface CreatePurchaseReturn {
  purchaseId: number;
  returnDate: string;
  returnNumber: string;
  notes?: string;
  purchaseReturnDetails: CreatePurchaseReturnDetail[];
}

export interface CreatePurchaseReturnDetail {
  purchaseDetailId: number;
  itemId: number;
  quantity: number;
  purchaseRate: number;
  costRate: number;
  retailRate: number;
  wholesaleRate: number;
  mrp: number;
  discount: number;
  taxPercentage: number;
  reason?: string;
}

export interface UpdatePurchaseReturn extends CreatePurchaseReturn {
  id: number;
}

export const purchaseReturnService = {
  async getPaged(pageNumber: number, pageSize: number, searchTerm?: string): Promise<ApiResponse<PagedResponse<PurchaseReturn>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      ...(searchTerm && { searchTerm })
    });
    const response = await api.get(`/purchasereturns?${params.toString()}`);
    return response.data;
  },

  async getById(id: number): Promise<ApiResponse<PurchaseReturn>> {
    const response = await api.get(`/purchasereturns/${id}`);
    return response.data;
  },

  async create(dto: CreatePurchaseReturn): Promise<ApiResponse<PurchaseReturn>> {
    const response = await api.post(`/purchasereturns`, dto);
    return response.data;
  },

  async update(dto: UpdatePurchaseReturn): Promise<ApiResponse> {
    const response = await api.put(`/purchasereturns/${dto.id}`, dto);
    return response.data;
  },

  async delete(id: number): Promise<ApiResponse> {
    const response = await api.delete(`/purchasereturns/${id}`);
    return response.data;
  }
};

export interface SaleReturn {
  id: number;
  saleId: number;
  returnDate: string;
  returnNumber: string;
  totalAmount: number;
  discount: number;
  taxAmount: number;
  netAmount: number;
  notes?: string;
  saleReturnDetails: SaleReturnDetail[];
}

export interface SaleReturnDetail {
  id: number;
  saleReturnId: number;
  saleDetailId: number;
  itemId: number;
  quantity: number;
  rate: number;
  discount: number;
  taxPercentage: number;
  taxAmount: number;
  total: number;
  reason?: string;
}

export interface CreateSaleReturn {
  saleId: number;
  returnDate: string;
  returnNumber: string;
  notes?: string;
  saleReturnDetails: CreateSaleReturnDetail[];
}

export interface CreateSaleReturnDetail {
  saleDetailId: number;
  itemId: number;
  quantity: number;
  rate: number;
  discount: number;
  taxPercentage: number;
  reason?: string;
}

export interface UpdateSaleReturn extends CreateSaleReturn {
  id: number;
}

export const saleReturnService = {
  async getPaged(pageNumber: number, pageSize: number, searchTerm?: string): Promise<ApiResponse<PagedResponse<SaleReturn>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      ...(searchTerm && { searchTerm })
    });
    const response = await api.get(`/salereturns?${params.toString()}`);
    return response.data;
  },

  async getById(id: number): Promise<ApiResponse<SaleReturn>> {
    const response = await api.get(`/salereturns/${id}`);
    return response.data;
  },

  async create(dto: CreateSaleReturn): Promise<ApiResponse<SaleReturn>> {
    const response = await api.post(`/salereturns`, dto);
    return response.data;
  },

  async update(dto: UpdateSaleReturn): Promise<ApiResponse> {
    const response = await api.put(`/salereturns/${dto.id}`, dto);
    return response.data;
  },

  async delete(id: number): Promise<ApiResponse> {
    const response = await api.delete(`/salereturns/${id}`);
    return response.data;
  }
};

export interface StockAdjustment {
  id: number;
  adjustmentDate: string;
  adjustmentNumber: string;
  notes?: string;
  stockAdjustmentDetails: StockAdjustmentDetail[];
}

export interface StockAdjustmentDetail {
  id: number;
  stockAdjustmentId: number;
  itemId: number;
  quantityIn: number;
  quantityOut: number;
  reason?: string;
}

export interface CreateStockAdjustment {
  adjustmentDate: string;
  adjustmentNumber: string;
  notes?: string;
  stockAdjustmentDetails: CreateStockAdjustmentDetail[];
}

export interface CreateStockAdjustmentDetail {
  itemId: number;
  quantityIn: number;
  quantityOut: number;
  reason?: string;
}

export interface UpdateStockAdjustment extends CreateStockAdjustment {
  id: number;
}

export const stockAdjustmentService = {
  async getPaged(pageNumber: number, pageSize: number, searchTerm?: string): Promise<ApiResponse<PagedResponse<StockAdjustment>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      ...(searchTerm && { searchTerm })
    });
    const response = await api.get(`/stockadjustments?${params.toString()}`);
    return response.data;
  },

  async getById(id: number): Promise<ApiResponse<StockAdjustment>> {
    const response = await api.get(`/stockadjustments/${id}`);
    return response.data;
  },

  async create(dto: CreateStockAdjustment): Promise<ApiResponse<StockAdjustment>> {
    const response = await api.post(`/stockadjustments`, dto);
    return response.data;
  },

  async update(dto: UpdateStockAdjustment): Promise<ApiResponse> {
    const response = await api.put(`/stockadjustments/${dto.id}`, dto);
    return response.data;
  },

  async delete(id: number): Promise<ApiResponse> {
    const response = await api.delete(`/stockadjustments/${id}`);
    return response.data;
  }
};

export interface Payment {
  id: number;
  paymentDate: string;
  paymentNumber: string;
  paymentType: number;
  partyType: number;
  partyId: number;
  amount: number;
  paymentMethod?: string;
  referenceNumber?: string;
  notes?: string;
}

export interface CreatePayment {
  paymentDate: string;
  paymentNumber: string;
  paymentType: number;
  partyType: number;
  partyId: number;
  amount: number;
  paymentMethod?: string;
  referenceNumber?: string;
  notes?: string;
}

export interface UpdatePayment extends CreatePayment {
  id: number;
}

export const paymentService = {
  async getPaged(pageNumber: number, pageSize: number, searchTerm?: string): Promise<ApiResponse<PagedResponse<Payment>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      ...(searchTerm && { searchTerm })
    });
    const response = await api.get(`/payments?${params.toString()}`);
    return response.data;
  },

  async getById(id: number): Promise<ApiResponse<Payment>> {
    const response = await api.get(`/payments/${id}`);
    return response.data;
  },

  async create(dto: CreatePayment): Promise<ApiResponse<Payment>> {
    const response = await api.post(`/payments`, dto);
    return response.data;
  },

  async update(dto: UpdatePayment): Promise<ApiResponse> {
    const response = await api.put(`/payments/${dto.id}`, dto);
    return response.data;
  },

  async delete(id: number): Promise<ApiResponse> {
    const response = await api.delete(`/payments/${id}`);
    return response.data;
  }
};

export interface StockTransfer {
  id: number;
  transferDate: string;
  transferNumber: string;
  fromLocation?: string;
  toLocation?: string;
  notes?: string;
  stockTransferDetails: StockTransferDetail[];
}

export interface StockTransferDetail {
  id: number;
  stockTransferId: number;
  itemId: number;
  quantity: number;
}

export interface CreateStockTransfer {
  transferDate: string;
  transferNumber: string;
  fromLocation?: string;
  toLocation?: string;
  notes?: string;
  stockTransferDetails: CreateStockTransferDetail[];
}

export interface CreateStockTransferDetail {
  itemId: number;
  quantity: number;
}

export interface UpdateStockTransfer extends CreateStockTransfer {
  id: number;
}

export const stockTransferService = {
  async getPaged(pageNumber: number, pageSize: number, searchTerm?: string): Promise<ApiResponse<PagedResponse<StockTransfer>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      ...(searchTerm && { searchTerm })
    });
    const response = await api.get(`/stocktransfers?${params.toString()}`);
    return response.data;
  },

  async getById(id: number): Promise<ApiResponse<StockTransfer>> {
    const response = await api.get(`/stocktransfers/${id}`);
    return response.data;
  },

  async create(dto: CreateStockTransfer): Promise<ApiResponse<StockTransfer>> {
    const response = await api.post(`/stocktransfers`, dto);
    return response.data;
  },

  async update(dto: UpdateStockTransfer): Promise<ApiResponse> {
    const response = await api.put(`/stocktransfers/${dto.id}`, dto);
    return response.data;
  },

  async delete(id: number): Promise<ApiResponse> {
    const response = await api.delete(`/stocktransfers/${id}`);
    return response.data;
  }
};
