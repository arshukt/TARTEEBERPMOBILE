<template>
  <div class="erp-page erp-card-stack">
    <h1 class="erp-page-heading">Dashboard</h1>

    <el-row :gutter="20" class="mb-6">
      <el-col :xs="24" :sm="12" :md="6">
        <el-card class="metric-card">
          <div class="metric-card-body">
            <div>
              <p class="metric-label">Total Sales</p>
              <p class="metric-value">{{ totalSales }}</p>
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
              <p class="metric-label">Total Purchases</p>
              <p class="metric-value">{{ totalPurchases }}</p>
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
            <div class="flex justify-between items-center">
              <span class="font-semibold">Recent Sales</span>
              <router-link to="/sales">View all</router-link>
            </div>
          </template>

          <div v-if="recentSales.length">
            <div v-for="sale in recentSales" :key="sale.id" class="recent-item">
              <div class="recent-top">
                <strong>{{ sale.invoiceNumber }}</strong>
                <span>{{ formatCurrency(sale.netAmount) }}</span>
              </div>

              <div class="recent-bottom">
                <span>{{ sale.customerName }}</span>
                <span>{{ sale.saleDate }}</span>
              </div>
            </div>
          </div>

          <el-empty v-else description="No sales yet" />
        </el-card>
      </el-col>

      <el-col :xs="24" :lg="12">
        <el-card>
          <template #header>
            <div class="flex justify-between items-center">
              <span class="font-semibold">Recent Purchases</span>
              <router-link to="/purchases">View all</router-link>
            </div>
          </template>

          <div v-if="recentPurchases.length">
            <div
              v-for="purchase in recentPurchases"
              :key="purchase.id"
              class="recent-item"
            >
              <div class="recent-top">
                <strong>{{ purchase.invoiceNumber }}</strong>
                <span>{{ formatCurrency(purchase.netAmount) }}</span>
              </div>

              <div class="recent-bottom">
                <span>{{ purchase.supplierName }}</span>
                <span>{{ purchase.purchaseDate }}</span>
              </div>
            </div>
          </div>

          <el-empty v-else description="No purchases yet" />
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import { Money, ShoppingCart, UserFilled, Box } from "@element-plus/icons-vue";
import { customerService } from "@/services/customers";
import { itemService } from "@/services/items";
import { saleService } from "@/services/sales";
import { purchaseService } from "@/services/purchases";
import type { Sale } from "@/services/sales";
import type { Purchase } from "@/services/purchases";

const totalCustomers = ref(0);
const totalItems = ref(0);
const totalSales = ref(0);
const totalPurchases = ref(0);
const recentSales = ref<(Sale & { customerName?: string })[]>([]);
const recentPurchases = ref<(Purchase & { supplierName?: string })[]>([]);

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "QAR",
    minimumFractionDigits: 2,
  }).format(value);
};

const formatDate = (dateString: string) => {
  if (!dateString) return "";
  return new Date(dateString).toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
};

const fetchDashboardData = async () => {
  try {
    const customersResponse = await customerService.getPaged(1, 1);
    if (customersResponse.success && customersResponse.data) {
      totalCustomers.value = customersResponse.data.totalCount;
    }

    const itemsResponse = await itemService.getPaged(1, 1);
    if (itemsResponse.success && itemsResponse.data) {
      totalItems.value = itemsResponse.data.totalCount;
    }

    const salesResponse = await saleService.getPaged(1, 1);
    if (salesResponse.success && salesResponse.data) {
      totalSales.value = salesResponse.data.totalCount;
    }

    const recentSalesResponse = await saleService.getPaged(1, 5);
    if (recentSalesResponse.success && recentSalesResponse.data) {
      recentSales.value = (recentSalesResponse.data.items || []).map(
        (sale) => ({
          ...sale,
          customerName: sale.customer?.customerName,
          saleDate: formatDate(sale.saleDate),
        }),
      );
    }

    const purchasesResponse = await purchaseService.getPaged(1, 1);
    if (purchasesResponse.success && purchasesResponse.data) {
      totalPurchases.value = purchasesResponse.data.totalCount;
    }

    const recentPurchasesResponse = await purchaseService.getPaged(1, 5);
    if (recentPurchasesResponse.success && recentPurchasesResponse.data) {
      recentPurchases.value = (recentPurchasesResponse.data.items || []).map(
        (purchase) => ({
          ...purchase,
          supplierName: purchase.supplier?.supplierName,
          purchaseDate: formatDate(purchase.purchaseDate),
        }),
      );
    }
  } catch (error) {
    console.error("Failed to fetch dashboard data:", error);
  }
};

onMounted(() => {
  fetchDashboardData();
});
</script>
