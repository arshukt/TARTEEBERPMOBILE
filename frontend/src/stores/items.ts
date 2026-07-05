import { defineStore } from "pinia";
import { ref } from "vue";
import { ElMessage } from "element-plus";
import type { Item, CreateItem, UpdateItem } from "@/services/items";
import { itemService } from "@/services/items";
import { reportService } from "@/services/reports";

export const useItemStore = defineStore("items", () => {
  const items = ref<Item[]>([]);
  const loading = ref(false);
  const pagedItems = ref<{
    items: Item[];
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
      const response = await itemService.getAll();
      if (response.success) {
        items.value = response.data || [];
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch items");
    } finally {
      loading.value = false;
    }
  };

  const fetchAllWithStock = async () => {
    try {
      loading.value = true;
      const [itemsResponse, stockResponse] = await Promise.all([
        itemService.getAll(),
        reportService.getCurrentStock()
      ]);
      if (itemsResponse.success) {
        const stockMap = new Map<number, number>();
        if (stockResponse.success && stockResponse.data) {
          stockResponse.data.forEach((s: any) => stockMap.set(s.id, s.currentStock));
        }
        items.value = (itemsResponse.data || []).map(item => ({
          ...item,
          currentStock: stockMap.get(item.id)
        }));
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch items");
    } finally {
      loading.value = false;
    }
  };

  const fetchPaged = async (pageNumber: number, pageSize: number, searchTerm?: string) => {
    try {
      loading.value = true;
      const response = await itemService.getPaged(pageNumber, pageSize, searchTerm);
      if (response.success) {
        pagedItems.value = response.data!;
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch items");
    } finally {
      loading.value = false;
    }
  };

  const create = async (dto: CreateItem) => {
    try {
      loading.value = true;
      const response = await itemService.create(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to create item");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const update = async (dto: UpdateItem) => {
    try {
      loading.value = true;
      const response = await itemService.update(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to update item");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const remove = async (id: number) => {
    try {
      loading.value = true;
      const response = await itemService.delete(id);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to delete item");
      return false;
    } finally {
      loading.value = false;
    }
  };

  return {
    items,
    pagedItems,
    loading,
    fetchAll,
    fetchAllWithStock,
    fetchPaged,
    create,
    update,
    remove
  };
});