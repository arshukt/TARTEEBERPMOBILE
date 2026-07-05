<template>
  <div class="erp-page erp-card-stack">
    <h1 class="erp-page-heading">Current Stock Report</h1>
    <el-card>
      <el-table :data="reportData" v-loading="loading" style="width: 100%">
        <el-table-column prop="code" label="Code" width="80" />
        <el-table-column prop="name" label="Item Name" width="150"/>
        <el-table-column prop="category" label="Category" width="120" />
        <el-table-column prop="currentStock" label="Stock" width="100" />
        <el-table-column prop="unit" label="Unit" width="100" />
      </el-table>
      <el-empty v-if="!reportData.length && !loading" description="No data available" />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { ElMessage } from "element-plus";
import { reportService, type CurrentStockReportItem } from "@/services/reports";

const loading = ref(false);
const reportData = ref<CurrentStockReportItem[]>([]);

const fetchReport = async () => {
  loading.value = true;
  try {
    const response = await reportService.getCurrentStock();
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