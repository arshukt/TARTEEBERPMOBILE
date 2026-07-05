<template>
  <div class="erp-page erp-card-stack">
    <h1 class="erp-page-heading">Sales Report</h1>
    <el-card>
      <div class="erp-toolbar">
        <el-date-picker
          v-model="dateRange"
          type="daterange"
          range-separator="To"
          start-placeholder="Start date"
          end-placeholder="End date"
          format="YYYY-MM-DD"
          value-format="YYYY-MM-DD"
        />
        <el-button type="primary" @click="fetchReport">Search</el-button>
      </div>
      <el-table :data="reportData" v-loading="loading" style="width: 100%">
        <el-table-column prop="date" label="Date" width="100">
          <template #default="{ row }">
            {{ formatDate(row.date) }}
          </template>
        </el-table-column>
        <el-table-column prop="invoiceNumber" label="Invoice" width="120" />
        <el-table-column prop="customer" label="Customer" width="120" />
        <el-table-column prop="total" label="Total" width="120">
          <template #default="{ row }">
            {{ formatCurrency(row.total) }}
          </template>
        </el-table-column>
      </el-table>
      <el-empty
        v-if="!reportData.length && !loading"
        description="No data available"
      />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { ElMessage } from "element-plus";
import { reportService, type SalesReportItem } from "@/services/reports";

const loading = ref(false);
const dateRange = ref<string[]>([]);
const reportData = ref<SalesReportItem[]>([]);

const formatDate = (date: string | Date) => {
  const d = new Date(date);
  return d.toLocaleDateString();
};

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat(undefined, {
    style: "currency",
    currency: "QAR",
  }).format(amount);
};

const fetchReport = async () => {
  loading.value = true;
  try {
    const startDate = dateRange.value?.[0];
    const endDate = dateRange.value?.[1];
    const response = await reportService.getSales(startDate, endDate);
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
