<template>
  <div class="erp-page erp-card-stack">
    <h1 class="erp-page-heading">Customer Outstanding</h1>
    <el-card>
      <el-table :data="reportData" v-loading="loading" style="width: 100%">
        <el-table-column prop="customerName" label="Customer" width="150" />
        <el-table-column prop="totalOutstanding" label="Outstanding" width="120">
          <template #default="{ row }">
            {{ formatCurrency(row.totalOutstanding) }}
          </template>
        </el-table-column>
        <el-table-column prop="lastTransaction" label="Last Transaction" width="150">
          <template #default="{ row }">
            {{ row.lastTransaction ? formatDate(row.lastTransaction) : '-' }}
          </template>
        </el-table-column>
      </el-table>
      <el-empty v-if="!reportData.length && !loading" description="No data available" />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { ElMessage } from "element-plus";
import { reportService, type CustomerOutstandingReportItem } from "@/services/reports";

const loading = ref(false);
const reportData = ref<CustomerOutstandingReportItem[]>([]);

const formatDate = (date: string | Date) => {
  const d = new Date(date);
  return d.toLocaleDateString();
};

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'QAR' }).format(amount);
};

const fetchReport = async () => {
  loading.value = true;
  try {
    const response = await reportService.getCustomerOutstanding();
    if (response.success && response.data) {
      reportData.value = response.data;
    } else {
      ElMessage.error(response.message || "Failed to fetch report");
    }
  } catch (error) {
    console.error("Failed to fetch report:", error);
    ElMessage.error("Failed to fetch report");
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchReport();
});
</script>