import { defineStore } from "pinia";
import { ref } from "vue";
import type { Company, CreateCompany, UpdateCompany } from "@/services/companies";
import { companyService } from "@/services/companies";
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