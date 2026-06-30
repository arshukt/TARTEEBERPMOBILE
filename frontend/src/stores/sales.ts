import { defineStore } from "pinia";
import { ref } from "vue";
import { ElMessage } from "element-plus";
import type { Sale, CreateSale, UpdateSale } from "@/services/sales";
import { saleService } from "@/services/sales";

export const useSaleStore = defineStore("sales", () => {
    const loading = ref(false);
    const pagedSales = ref<{
        items: Sale[];
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

    const fetchPaged = async (pageNumber: number, pageSize: number, searchTerm?: string) => {
        try {
            loading.value = true;
            const response = await saleService.getPaged(pageNumber, pageSize, searchTerm);
            if (response.success) {
                pagedSales.value = response.data!;
            }
        } catch (error: any) {
            ElMessage.error(error?.response?.data?.message || "Failed to fetch sales");
        } finally {
            loading.value = false;
        }
    };

    const create = async (dto: CreateSale) => {
        try {
            loading.value = true;
            const response = await saleService.create(dto);
            if (response.success) {
                ElMessage.success(response.message);
                return true;
            }
            return false;
        } catch (error: any) {
            ElMessage.error(error?.response?.data?.message || "Failed to create sale");
            return false;
        } finally {
            loading.value = false;
        }
    };

    const update = async (dto: UpdateSale) => {
        try {
            loading.value = true;
            const response = await saleService.update(dto);
            if (response.success) {
                ElMessage.success(response.message);
                return true;
            }
            return false;
        } catch (error: any) {
            ElMessage.error(error?.response?.data?.message || "Failed to update sale");
            return false;
        } finally {
            loading.value = false;
        }
    };

    const remove = async (id: number) => {
        try {
            loading.value = true;
            const response = await saleService.delete(id);
            if (response.success) {
                ElMessage.success(response.message);
                return true;
            }
            return false;
        } catch (error: any) {
            ElMessage.error(error?.response?.data?.message || "Failed to delete sale");
            return false;
        } finally {
            loading.value = false;
        }
    };

    return {
        loading,
        pagedSales,
        fetchPaged,
        create,
        update,
        remove
    };
});
