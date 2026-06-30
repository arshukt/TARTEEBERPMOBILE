import { defineStore } from "pinia";
import { ref } from "vue";
import { ElMessage } from "element-plus";
import type { Customer, CreateCustomer, UpdateCustomer } from "@/services/customers";
import { customerService } from "@/services/customers";

export const useCustomerStore = defineStore("customers", () => {
  const customers = ref<Customer[]>([]);
  const loading = ref(false);
  const pagedCustomers = ref<{
    items: Customer[];
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
      const response = await customerService.getAll();
      if (response.success) {
        customers.value = response.data || [];
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch customers");
    } finally {
      loading.value = false;
    }
  };

  const fetchPaged = async (pageNumber: number, pageSize: number, searchTerm?: string) => {
    try {
      loading.value = true;
      const response = await customerService.getPaged(pageNumber, pageSize, searchTerm);
      if (response.success) {
        pagedCustomers.value = response.data!;
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch customers");
    } finally {
      loading.value = false;
    }
  };

  const create = async (dto: CreateCustomer) => {
    try {
      loading.value = true;
      const response = await customerService.create(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to create customer");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const update = async (dto: UpdateCustomer) => {
    try {
      loading.value = true;
      const response = await customerService.update(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to update customer");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const remove = async (id: number) => {
    try {
      loading.value = true;
      const response = await customerService.delete(id);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to delete customer");
      return false;
    } finally {
      loading.value = false;
    }
  };

  return {
    customers,
    pagedCustomers,
    loading,
    fetchAll,
    fetchPaged,
    create,
    update,
    remove
  };
});