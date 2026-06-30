import { defineStore } from "pinia";
import { ref } from "vue";
import type { StockTransaction } from "@/services/stockTransactions";
import { stockTransactionService } from "@/services/stockTransactions";
import { ElMessage } from "element-plus";

export const useStockTransactionStore = defineStore("stockTransactions", () => {
  const stockTransactions = ref<StockTransaction[]>([]);
  const loading = ref(false);
  const pagedStockTransactions = ref<{
    items: StockTransaction[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasPrevious: boolean;
    hasNext: boolean;
  }>({
    items: [],
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
    hasPrevious: false,
    hasNext: false
  });

  const fetchPaged = async (pageNumber: number, pageSize: number, searchTerm?: string, itemId?: number) => {
    try {
      loading.value = true;
      const response = await stockTransactionService.getPaged(pageNumber, pageSize, searchTerm, itemId);
      if (response.success) {
        pagedStockTransactions.value = response.data!;
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch stock transactions");
    } finally {
      loading.value = false;
    }
  };

  return {
    stockTransactions,
    pagedStockTransactions,
    loading,
    fetchPaged
  };
});
