<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-6">Reports</h1>
    <el-row :gutter="20">
      <el-col :span="6" v-for="report in reports" :key="report.path">
        <el-card class="cursor-pointer hover:shadow-lg transition-shadow" @click="goToReport(report.path)">
          <div class="flex items-center gap-3">
            <el-icon :size="32" :color="report.color">
              <component :is="report.icon" />
            </el-icon>
            <div>
              <div class="font-semibold">{{ report.title }}</div>
              <div class="text-sm text-gray-500">{{ report.description }}</div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import { Document, ShoppingCart, Money, Box } from '@element-plus/icons-vue';

const router = useRouter();

const reports = [
  {
    title: 'Sales Report',
    description: 'View sales transactions by date, customer, or user',
    icon: Money,
    path: '/reports/sales',
    color: '#409EFF'
  },
  {
    title: 'Purchase Report',
    description: 'View purchase transactions by date or supplier',
    icon: ShoppingCart,
    path: '/reports/purchases',
    color: '#67C23A'
  },
  {
    title: 'Current Stock Report',
    description: 'View current stock levels and values',
    icon: Box,
    path: '/reports/current-stock',
    color: '#E6A23C'
  },
  {
    title: 'Customer Outstanding',
    description: 'View outstanding balances for customers',
    icon: Document,
    path: '/reports/customer-outstanding',
    color: '#F56C6C'
  },
  {
    title: 'Supplier Outstanding',
    description: 'View outstanding balances for suppliers',
    icon: Document,
    path: '/reports/supplier-outstanding',
    color: '#909399'
  }
];

const goToReport = (path: string) => {
  router.push(path);
};
</script>
