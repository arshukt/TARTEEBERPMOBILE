import { defineStore } from "pinia";
import { ref } from "vue";
import { ElMessage } from "element-plus";
import type { OpeningStock, CreateOpeningStock, UpdateOpeningStock, OpeningStockDetail } from "@/services/opening-stocks";
import { openingStockService } from "@/services/opening-stocks";

export const useOpeningStockStore = defineStore("openingStocks", () => {
    const loading = ref(false);
    const pagedOpeningStocks = ref<{
        items: OpeningStock[];
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
            const response = await openingStockService.getPaged(pageNumber, pageSize, searchTerm);
            if (response.success) {
                const items = (response.data!.items || []).map((item: OpeningStock) => ({
                    ...item,
                    quantity: item.openingStockDetails?.reduce((sum: number, d: OpeningStockDetail) => sum + d.quantity, 0)
                }));
                pagedOpeningStocks.value = {
                    ...response.data!,
                    items
                };
            }
        } catch (error: any) {
            ElMessage.error(error?.response?.data?.message || "Failed to fetch opening stocks");
        } finally {
            loading.value = false;
        }
    };

    const create = async (dto: CreateOpeningStock) => {
        try {
            loading.value = true;
            const response = await openingStockService.create(dto);
            if (response.success) {
                ElMessage.success(response.message);
                return true;
            }
            return false;
        } catch (error: any) {
            ElMessage.error(error?.response?.data?.message || "Failed to create opening stock");
            return false;
        } finally {
            loading.value = false;
        }
    };

    const update = async (dto: UpdateOpeningStock) => {
        try {
            loading.value = true;
            const response = await openingStockService.update(dto);
            if (response.success) {
                ElMessage.success(response.message);
                return true;
            }
            return false;
        } catch (error: any) {
            ElMessage.error(error?.response?.data?.message || "Failed to update opening stock");
            return false;
        } finally {
            loading.value = false;
        }
    };

    const remove = async (id: number) => {
        try {
            loading.value = true;
            const response = await openingStockService.delete(id);
            if (response.success) {
                ElMessage.success(response.message);
                return true;
            }
            return false;
        } catch (error: any) {
            ElMessage.error(error?.response?.data?.message || "Failed to delete opening stock");
            return false;
        } finally {
            loading.value = false;
        }
    };

    return {
        loading,
        pagedOpeningStocks,
        fetchPaged,
        create,
        update,
        remove
    };
});
