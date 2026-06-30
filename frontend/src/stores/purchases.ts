import { defineStore } from "pinia";
import { ref } from "vue";
import { ElMessage } from "element-plus";
import type { Purchase, CreatePurchase, UpdatePurchase } from "@/services/purchases";
import { purchaseService } from "@/services/purchases";

export const usePurchaseStore = defineStore("purchases", () => {
    const loading = ref(false);
    const pagedPurchases = ref<{
        items: Purchase[];
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
            const response = await purchaseService.getPaged(pageNumber, pageSize, searchTerm);
            if (response.success) {
                pagedPurchases.value = response.data!;
            }
        } catch (error: any) {
            ElMessage.error(error?.response?.data?.message || "Failed to fetch purchases");
        } finally {
            loading.value = false;
        }
    };

    const create = async (dto: CreatePurchase) => {
        try {
            loading.value = true;
            const response = await purchaseService.create(dto);
            if (response.success) {
                ElMessage.success(response.message);
                return true;
            }
            return false;
        } catch (error: any) {
            ElMessage.error(error?.response?.data?.message || "Failed to create purchase");
            return false;
        } finally {
            loading.value = false;
        }
    };

    const update = async (dto: UpdatePurchase) => {
        try {
            loading.value = true;
            const response = await purchaseService.update(dto);
            if (response.success) {
                ElMessage.success(response.message);
                return true;
            }
            return false;
        } catch (error: any) {
            ElMessage.error(error?.response?.data?.message || "Failed to update purchase");
            return false;
        } finally {
            loading.value = false;
        }
    };

    const remove = async (id: number) => {
        try {
            loading.value = true;
            const response = await purchaseService.delete(id);
            if (response.success) {
                ElMessage.success(response.message);
                return true;
            }
            return false;
        } catch (error: any) {
            ElMessage.error(error?.response?.data?.message || "Failed to delete purchase");
            return false;
        } finally {
            loading.value = false;
        }
    };

    return {
        loading,
        pagedPurchases,
        fetchPaged,
        create,
        update,
        remove
    };
});
