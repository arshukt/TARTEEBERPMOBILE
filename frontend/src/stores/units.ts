import { defineStore } from "pinia";
import { ref } from "vue";
import type { Unit, CreateUnit, UpdateUnit } from "@/services/units";
import { unitService } from "@/services/units";
import { ElMessage } from "element-plus";

export const useUnitStore = defineStore("units", () => {
  const units = ref<Unit[]>([]);
  const loading = ref(false);
  const pagedUnits = ref<{
    items: Unit[];
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
      const response = await unitService.getAll();
      if (response.success) {
        units.value = response.data || [];
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch units");
    } finally {
      loading.value = false;
    }
  };

  const fetchPaged = async (pageNumber: number, pageSize: number, searchTerm?: string) => {
    try {
      loading.value = true;
      const response = await unitService.getPaged(pageNumber, pageSize, searchTerm);
      if (response.success) {
        pagedUnits.value = response.data!;
      }
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to fetch units");
    } finally {
      loading.value = false;
    }
  };

  const create = async (dto: CreateUnit) => {
    try {
      loading.value = true;
      const response = await unitService.create(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to create unit");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const update = async (dto: UpdateUnit) => {
    try {
      loading.value = true;
      const response = await unitService.update(dto);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to update unit");
      return false;
    } finally {
      loading.value = false;
    }
  };

  const remove = async (id: number) => {
    try {
      loading.value = true;
      const response = await unitService.delete(id);
      if (response.success) {
        ElMessage.success(response.message);
        return true;
      }
      return false;
    } catch (error: any) {
      ElMessage.error(error?.response?.data?.message || "Failed to delete unit");
      return false;
    } finally {
      loading.value = false;
    }
  };

  return {
    units,
    pagedUnits,
    loading,
    fetchAll,
    fetchPaged,
    create,
    update,
    remove
  };
});
