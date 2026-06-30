<template>
  <div class="erp-page erp-card-stack">
    <h1 class="erp-page-heading">Dashboard</h1>

    <el-row :gutter="20" class="mb-6">
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="metric-card">
          <div class="metric-card-body">
            <div>
              <p class="metric-label">Today's Sales</p>
              <p class="metric-value">QAR 0.00</p>
            </div>
            <div class="metric-icon accent">
              <el-icon :size="24"><Money /></el-icon>
            </div>
          </div>
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="metric-card">
          <div class="metric-card-body">
            <div>
              <p class="metric-label">Today's Purchases</p>
              <p class="metric-value">QAR 0.00</p>
            </div>
            <div class="metric-icon">
              <el-icon :size="24"><ShoppingCart /></el-icon>
            </div>
          </div>
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="metric-card">
          <div class="metric-card-body">
            <div>
              <p class="metric-label">Total Customers</p>
              <p class="metric-value">{{ totalCustomers }}</p>
            </div>
            <div class="metric-icon">
              <el-icon :size="24"><UserFilled /></el-icon>
            </div>
          </div>
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="metric-card">
          <div class="metric-card-body">
            <div>
              <p class="metric-label">Total Items</p>
              <p class="metric-value">{{ totalItems }}</p>
            </div>
            <div class="metric-icon warning">
              <el-icon :size="24"><Box /></el-icon>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="20">
      <el-col :xs="24" :lg="12">
        <el-card>
          <template #header>
            <div class="flex items-center justify-between">
              <span class="font-semibold">Recent Sales</span>
            </div>
          </template>
          <el-empty description="No sales yet" />
        </el-card>
      </el-col>

      <el-col :xs="24" :lg="12">
        <el-card>
          <template #header>
            <div class="flex items-center justify-between">
              <span class="font-semibold">Recent Purchases</span>
            </div>
          </template>
          <el-empty description="No purchases yet" />
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Money, ShoppingCart, UserFilled, Box } from '@element-plus/icons-vue'
import { customerService } from '@/services/customers'
import { itemService } from '@/services/items'

const totalCustomers = ref(0)
const totalItems = ref(0)

const fetchDashboardData = async () => {
  try {
    const customersResponse = await customerService.getPaged(1, 1)
    if (customersResponse.success && customersResponse.data) {
      totalCustomers.value = customersResponse.data.totalCount
    }

    const itemsResponse = await itemService.getPaged(1, 1)
    if (itemsResponse.success && itemsResponse.data) {
      totalItems.value = itemsResponse.data.totalCount
    }
  } catch (error) {
    console.error('Failed to fetch dashboard data:', error)
  }
}

onMounted(() => {
  fetchDashboardData()
})
</script>
