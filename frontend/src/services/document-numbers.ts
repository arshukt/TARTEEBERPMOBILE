import api from "./api";
import type { ApiResponse } from "@/types";

export type DocumentType =
  | "purchase"
  | "sale"
  | "purchase-return"
  | "sale-return"
  | "stock-adjustment"
  | "payment"
  | "stock-transfer"
  | "opening-stock";

export const documentNumberService = {
  async getNext(documentType: DocumentType): Promise<ApiResponse<string>> {
    const response = await api.get(`/documentnumbers/${documentType}/next`);
    return response.data;
  }
};
