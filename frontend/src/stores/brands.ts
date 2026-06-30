import { defineStore } from "pinia";
import { ref } from "vue";
import type { Brand, CreateBrand, UpdateBrand } from "@/services/brands";
import { brandService } from "@/services/brands";
import { ElMessage } from "element-plus";

export const useBrandStore = defineStore("brands", () => {
  const brands = ref<Brand[]>([]);
  const loading = ref(false);
  const pagedBrands = ref<{
    items: Brand[];
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
      const response = await brandService.getAll();
      if (response.success) {
        brands.value = response.data || [];
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch brands");
    } finally {
      loading.value = false;
    }
  };

  const fetchPaged = async (pageNumber: number, pageSize: number, searchTerm?: string) => {
    try {
      loading.value = true;
      const response = await brandService.getPaged(pageNumber, pageSize, searchTerm);
      if (response.success) {
        pagedBrands.value = response.data!;
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch brands");
    } finally {
      loading.value = false;
    }
  };

  const create = async (dto: CreateBrand) => {
    try {
      loading.value = true;
      const response = await brandService.create(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to create brand");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const update = async (dto: UpdateBrand) => {
    try {
      loading.value = true;
      const response = await brandService.update(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to update brand");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const remove = async (id: number) => {
    try {
      loading.value = true;
      const response = await brandService.delete(id);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to delete brand");
      return false;
    } finally {
      loading.value = false;
    }
  };

  return {
    brands,
    pagedBrands,
    loading,
    fetchAll,
    fetchPaged,
    create,
    update,
    remove
  };
});
