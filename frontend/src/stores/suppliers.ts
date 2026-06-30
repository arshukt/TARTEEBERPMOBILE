import { defineStore } from "pinia";
import { ref } from "vue";
import { ElMessage } from "element-plus";
import type { Supplier, CreateSupplier, UpdateSupplier } from "@/services/suppliers";
import { supplierService } from "@/services/suppliers";

export const useSupplierStore = defineStore("suppliers", () => {
  const suppliers = ref<Supplier[]>([]);
  const loading = ref(false);
  const pagedSuppliers = ref<{
    items: Supplier[];
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
      const response = await supplierService.getAll();
      if (response.success) {
        suppliers.value = response.data || [];
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch suppliers");
    } finally {
      loading.value = false;
    }
  };

  const fetchPaged = async (pageNumber: number, pageSize: number, searchTerm?: string) => {
    try {
      loading.value = true;
      const response = await supplierService.getPaged(pageNumber, pageSize, searchTerm);
      if (response.success) {
        pagedSuppliers.value = response.data!;
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch suppliers");
    } finally {
      loading.value = false;
    }
  };

  const create = async (dto: CreateSupplier) => {
    try {
      loading.value = true;
      const response = await supplierService.create(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to create supplier");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const update = async (dto: UpdateSupplier) => {
    try {
      loading.value = true;
      const response = await supplierService.update(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to update supplier");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const remove = async (id: number) => {
    try {
      loading.value = true;
      const response = await supplierService.delete(id);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to delete supplier");
      return false;
    } finally {
      loading.value = false;
    }
  };

  return {
    suppliers,
    pagedSuppliers,
    loading,
    fetchAll,
    fetchPaged,
    create,
    update,
    remove
  };
});