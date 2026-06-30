import { defineStore } from "pinia";
import { ref } from "vue";
import type { Category, CreateCategory, UpdateCategory } from "@/services/categories";
import { categoryService } from "@/services/categories";
import { ElMessage } from "element-plus";

export const useCategoryStore = defineStore("categories", () => {
  const categories = ref<Category[]>([]);
  const loading = ref(false);
  const pagedCategories = ref<{
    items: Category[];
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
      const response = await categoryService.getAll();
      if (response.success) {
        categories.value = response.data || [];
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch categories");
    } finally {
      loading.value = false;
    }
  };

  const fetchPaged = async (pageNumber: number, pageSize: number, searchTerm?: string) => {
    try {
      loading.value = true;
      const response = await categoryService.getPaged(pageNumber, pageSize, searchTerm);
      if (response.success) {
        pagedCategories.value = response.data!;
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch categories");
    } finally {
      loading.value = false;
    }
  };

  const create = async (dto: CreateCategory) => {
    try {
      loading.value = true;
      const response = await categoryService.create(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to create category");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const update = async (dto: UpdateCategory) => {
    try {
      loading.value = true;
      const response = await categoryService.update(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to update category");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const remove = async (id: number) => {
    try {
      loading.value = true;
      const response = await categoryService.delete(id);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to delete category");
      return false;
    } finally {
      loading.value = false;
    }
  };

  return {
    categories,
    pagedCategories,
    loading,
    fetchAll,
    fetchPaged,
    create,
    update,
    remove
  };
});
