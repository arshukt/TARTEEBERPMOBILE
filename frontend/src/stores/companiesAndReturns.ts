import { defineStore } from "pinia";
import { ref } from "vue";
import type { Company, CreateCompany, UpdateCompany, PurchaseReturn, CreatePurchaseReturn, UpdatePurchaseReturn, SaleReturn, CreateSaleReturn, UpdateSaleReturn, StockAdjustment, CreateStockAdjustment, UpdateStockAdjustment, Payment, CreatePayment, UpdatePayment, StockTransfer, CreateStockTransfer, UpdateStockTransfer } from "@/services/companiesAndReturns";
import { companyService, purchaseReturnService, saleReturnService, stockAdjustmentService, paymentService, stockTransferService } from "@/services/companiesAndReturns";
import { ElMessage } from "element-plus";

export const useCompanyStore = defineStore("companies", () => {
  const companies = ref<Company[]>([]);
  const currentCompany = ref<Company | null>(null);
  const loading = ref(false);
  const pagedCompanies = ref<{
    items: Company[];
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

  const fetchAll = async () => {
    try {
      loading.value = true;
      const response = await companyService.getAll();
      if (response.success) {
        companies.value = response.data || [];
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch companies");
    } finally {
      loading.value = false;
    }
  };

  const fetchFirst = async () => {
    try {
      loading.value = true;
      const response = await companyService.getFirst();
      if (response.success && response.data) {
        currentCompany.value = response.data;
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch company");
    } finally {
      loading.value = false;
    }
  };

  const fetchPaged = async (pageNumber: number, pageSize: number, searchTerm?: string) => {
    try {
      loading.value = true;
      const response = await companyService.getPaged(pageNumber, pageSize, searchTerm);
      if (response.success) {
        pagedCompanies.value = response.data!;
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch companies");
    } finally {
      loading.value = false;
    }
  };

  const create = async (dto: CreateCompany) => {
    try {
      loading.value = true;
      const response = await companyService.create(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to create company");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const update = async (dto: UpdateCompany) => {
    try {
      loading.value = true;
      const response = await companyService.update(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to update company");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const remove = async (id: number) => {
    try {
      loading.value = true;
      const response = await companyService.delete(id);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to delete company");
      return false;
    } finally {
      loading.value = false;
    }
  };

  return {
    companies,
    currentCompany,
    pagedCompanies,
    loading,
    fetchAll,
    fetchFirst,
    fetchPaged,
    create,
    update,
    remove
  };
});

export const usePurchaseReturnStore = defineStore("purchaseReturns", () => {
  const purchaseReturns = ref<PurchaseReturn[]>([]);
  const loading = ref(false);
  const pagedPurchaseReturns = ref<{
    items: PurchaseReturn[];
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

  const fetchAll = async () => {
    try {
      loading.value = true;
      // Note: If getAll is not implemented, we can use fetchPaged with large pageSize
      const response = await purchaseReturnService.getPaged(1, 1000);
      if (response.success) {
        purchaseReturns.value = response.data?.items || [];
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch purchase returns");
    } finally {
      loading.value = false;
    }
  };

  const fetchPaged = async (pageNumber: number, pageSize: number, searchTerm?: string) => {
    try {
      loading.value = true;
      const response = await purchaseReturnService.getPaged(pageNumber, pageSize, searchTerm);
      if (response.success) {
        pagedPurchaseReturns.value = response.data!;
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch purchase returns");
    } finally {
      loading.value = false;
    }
  };

  const create = async (dto: CreatePurchaseReturn) => {
    try {
      loading.value = true;
      const response = await purchaseReturnService.create(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to create purchase return");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const update = async (dto: UpdatePurchaseReturn) => {
    try {
      loading.value = true;
      const response = await purchaseReturnService.update(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to update purchase return");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const remove = async (id: number) => {
    try {
      loading.value = true;
      const response = await purchaseReturnService.delete(id);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to delete purchase return");
      return false;
    } finally {
      loading.value = false;
    }
  };

  return {
    purchaseReturns,
    pagedPurchaseReturns,
    loading,
    fetchAll,
    fetchPaged,
    create,
    update,
    remove
  };
});

export const useSaleReturnStore = defineStore("saleReturns", () => {
  const saleReturns = ref<SaleReturn[]>([]);
  const loading = ref(false);
  const pagedSaleReturns = ref<{
    items: SaleReturn[];
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

  const fetchAll = async () => {
    try {
      loading.value = true;
      // Note: If getAll is not implemented, we can use fetchPaged with large pageSize
      const response = await saleReturnService.getPaged(1, 1000);
      if (response.success) {
        saleReturns.value = response.data?.items || [];
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch sale returns");
    } finally {
      loading.value = false;
    }
  };

  const fetchPaged = async (pageNumber: number, pageSize: number, searchTerm?: string) => {
    try {
      loading.value = true;
      const response = await saleReturnService.getPaged(pageNumber, pageSize, searchTerm);
      if (response.success) {
        pagedSaleReturns.value = response.data!;
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch sale returns");
    } finally {
      loading.value = false;
    }
  };

  const create = async (dto: CreateSaleReturn) => {
    try {
      loading.value = true;
      const response = await saleReturnService.create(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to create sale return");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const update = async (dto: UpdateSaleReturn) => {
    try {
      loading.value = true;
      const response = await saleReturnService.update(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to update sale return");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const remove = async (id: number) => {
    try {
      loading.value = true;
      const response = await saleReturnService.delete(id);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to delete sale return");
      return false;
    } finally {
      loading.value = false;
    }
  };

  return {
    saleReturns,
    pagedSaleReturns,
    loading,
    fetchAll,
    fetchPaged,
    create,
    update,
    remove
  };
});

export const useStockAdjustmentStore = defineStore("stockAdjustments", () => {
  const stockAdjustments = ref<StockAdjustment[]>([]);
  const loading = ref(false);
  const pagedStockAdjustments = ref<{
    items: StockAdjustment[];
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

  const fetchAll = async () => {
    try {
      loading.value = true;
      // Note: If getAll is not implemented, we can use fetchPaged with large pageSize
      const response = await stockAdjustmentService.getPaged(1, 1000);
      if (response.success) {
        stockAdjustments.value = response.data?.items || [];
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch stock adjustments");
    } finally {
      loading.value = false;
    }
  };

  const fetchPaged = async (pageNumber: number, pageSize: number, searchTerm?: string) => {
    try {
      loading.value = true;
      const response = await stockAdjustmentService.getPaged(pageNumber, pageSize, searchTerm);
      if (response.success) {
        pagedStockAdjustments.value = response.data!;
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch stock adjustments");
    } finally {
      loading.value = false;
    }
  };

  const create = async (dto: CreateStockAdjustment) => {
    try {
      loading.value = true;
      const response = await stockAdjustmentService.create(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to create stock adjustment");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const update = async (dto: UpdateStockAdjustment) => {
    try {
      loading.value = true;
      const response = await stockAdjustmentService.update(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to update stock adjustment");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const remove = async (id: number) => {
    try {
      loading.value = true;
      const response = await stockAdjustmentService.delete(id);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to delete stock adjustment");
      return false;
    } finally {
      loading.value = false;
    }
  };

  return {
    stockAdjustments,
    pagedStockAdjustments,
    loading,
    fetchAll,
    fetchPaged,
    create,
    update,
    remove
  };
});

export const usePaymentStore = defineStore("payments", () => {
  const payments = ref<Payment[]>([]);
  const loading = ref(false);
  const pagedPayments = ref<{
    items: Payment[];
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

  const fetchAll = async () => {
    try {
      loading.value = true;
      // Note: If getAll is not implemented, we can use fetchPaged with large pageSize
      const response = await paymentService.getPaged(1, 1000);
      if (response.success) {
        payments.value = response.data?.items || [];
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch payments");
    } finally {
      loading.value = false;
    }
  };

  const fetchPaged = async (pageNumber: number, pageSize: number, searchTerm?: string) => {
    try {
      loading.value = true;
      const response = await paymentService.getPaged(pageNumber, pageSize, searchTerm);
      if (response.success) {
        pagedPayments.value = response.data!;
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch payments");
    } finally {
      loading.value = false;
    }
  };

  const create = async (dto: CreatePayment) => {
    try {
      loading.value = true;
      const response = await paymentService.create(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to create payment");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const update = async (dto: UpdatePayment) => {
    try {
      loading.value = true;
      const response = await paymentService.update(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to update payment");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const remove = async (id: number) => {
    try {
      loading.value = true;
      const response = await paymentService.delete(id);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to delete payment");
      return false;
    } finally {
      loading.value = false;
    }
  };

  return {
    payments,
    pagedPayments,
    loading,
    fetchAll,
    fetchPaged,
    create,
    update,
    remove
  };
});

export const useStockTransferStore = defineStore("stockTransfers", () => {
  const stockTransfers = ref<StockTransfer[]>([]);
  const loading = ref(false);
  const pagedStockTransfers = ref<{
    items: StockTransfer[];
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

  const fetchAll = async () => {
    try {
      loading.value = true;
      const response = await stockTransferService.getPaged(1, 1000);
      if (response.success) {
        stockTransfers.value = response.data?.items || [];
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch stock transfers");
    } finally {
      loading.value = false;
    }
  };

  const fetchPaged = async (pageNumber: number, pageSize: number, searchTerm?: string) => {
    try {
      loading.value = true;
      const response = await stockTransferService.getPaged(pageNumber, pageSize, searchTerm);
      if (response.success) {
        pagedStockTransfers.value = response.data!;
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch stock transfers");
    } finally {
      loading.value = false;
    }
  };

  const create = async (dto: CreateStockTransfer) => {
    try {
      loading.value = true;
      const response = await stockTransferService.create(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to create stock transfer");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const update = async (dto: UpdateStockTransfer) => {
    try {
      loading.value = true;
      const response = await stockTransferService.update(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to update stock transfer");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const remove = async (id: number) => {
    try {
      loading.value = true;
      const response = await stockTransferService.delete(id);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to delete stock transfer");
      return false;
    } finally {
      loading.value = false;
    }
  };

  return {
    stockTransfers,
    pagedStockTransfers,
    loading,
    fetchAll,
    fetchPaged,
    create,
    update,
    remove
  };
});
