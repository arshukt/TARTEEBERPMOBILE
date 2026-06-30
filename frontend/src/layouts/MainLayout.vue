<template>
  <el-container class="erp-shell">
    <el-aside
      v-if="!isMobile || sidebarVisible"
      :width="isMobile ? '280px' : '250px'"
      class="erp-sidebar fixed md:relative inset-0 z-50 md:z-auto transition-transform duration-300"
      :class="{
        '-translate-x-full md:translate-x-0': isMobile && !sidebarVisible,
      }"
    >
      <div class="erp-sidebar-header">
        <div class="erp-brand">
          <div class="erp-brand-mark">T</div>
          <div>
            <h1 class="erp-brand-name">Tarteeb ERP</h1>
            <p class="erp-brand-subtitle">Van Sales</p>
          </div>
        </div>
        <el-button v-if="isMobile" @click="sidebarVisible = false" link>
          <el-icon :size="20"><Close /></el-icon>
        </el-button>
      </div>
      <el-menu
        :default-active="activeMenu"
        router
        class="border-none h-[calc(100vh-65px)] md:h-[calc(100vh-64px)] overflow-y-auto"
        @select="handleMenuSelect"
      >
        <el-menu-item index="/" v-if="hasPermission('Dashboard')">
          <el-icon><HomeFilled /></el-icon>
          <span>Dashboard</span>
        </el-menu-item>
        <el-menu-item index="/company" v-if="hasPermission('Company')">
          <el-icon><OfficeBuilding /></el-icon>
          <span>Company</span>
        </el-menu-item>

        <el-sub-menu
          index="settings-sub"
          v-if="hasPermission('Roles') || hasPermission('Users')"
        >
          <template #title>
            <el-icon><User /></el-icon>
            <span>System Settings</span>
          </template>
          <el-menu-item index="/roles" v-if="hasPermission('Roles')"
            >Roles</el-menu-item
          >
          <el-menu-item index="/users" v-if="hasPermission('Users')"
            >Users</el-menu-item
          >
        </el-sub-menu>

        <el-menu-item index="/customers" v-if="hasPermission('Customers')">
          <el-icon><User /></el-icon>
          <span>Customers</span>
        </el-menu-item>
        <el-menu-item index="/suppliers" v-if="hasPermission('Suppliers')">
          <el-icon><Box /></el-icon>
          <span>Suppliers</span>
        </el-menu-item>
        <el-menu-item index="/categories" v-if="hasPermission('Categories')">
          <el-icon><FolderOpened /></el-icon>
          <span>Categories</span>
        </el-menu-item>
        <el-menu-item index="/brands" v-if="hasPermission('Brands')">
          <el-icon><Coin /></el-icon>
          <span>Brands</span>
        </el-menu-item>
        <el-menu-item index="/units" v-if="hasPermission('Units')">
          <el-icon><ScaleToOriginal /></el-icon>
          <span>Units</span>
        </el-menu-item>
        <el-menu-item index="/items" v-if="hasPermission('Items')">
          <el-icon><Box /></el-icon>
          <span>Items</span>
        </el-menu-item>
        <el-menu-item
          index="/opening-stocks"
          v-if="hasPermission('OpeningStocks')"
        >
          <el-icon><Document /></el-icon>
          <span>Opening Stock</span>
        </el-menu-item>
        <el-sub-menu
          index="purchases-sub"
          v-if="hasPermission('Purchases') || hasPermission('PurchaseReturns')"
        >
          <template #title>
            <el-icon><ShoppingCart /></el-icon>
            <span>Purchases</span>
          </template>
          <el-menu-item index="/purchases" v-if="hasPermission('Purchases')"
            >Purchases</el-menu-item
          >
          <el-menu-item
            index="/purchase-returns"
            v-if="hasPermission('PurchaseReturns')"
            >Purchase Returns</el-menu-item
          >
        </el-sub-menu>
        <el-sub-menu
          index="sales-sub"
          v-if="hasPermission('Sales') || hasPermission('SaleReturns')"
        >
          <template #title>
            <el-icon><Money /></el-icon>
            <span>Sales</span>
          </template>
          <el-menu-item index="/sales" v-if="hasPermission('Sales')"
            >Sales</el-menu-item
          >
          <el-menu-item
            index="/sale-returns"
            v-if="hasPermission('SaleReturns')"
            >Sales Returns</el-menu-item
          >
        </el-sub-menu>
        <!-- <el-menu-item
          index="/stock-adjustments"
          v-if="hasPermission('StockAdjustments')"
        >
          <el-icon><RefreshLeft /></el-icon>
          <span>Stock Adjustments</span>
        </el-menu-item> -->
        <!-- <el-menu-item
          index="/stock-transfers"
          v-if="hasPermission('StockTransfers')"
        >
          <el-icon><Switch /></el-icon>
          <span>Stock Transfers</span>
        </el-menu-item> -->
        <el-menu-item index="/stock-ledger" v-if="hasPermission('StockLedger')">
          <el-icon><Document /></el-icon>
          <span>Stock Ledger</span>
        </el-menu-item>
        <el-menu-item index="/payments" v-if="hasPermission('Payments')">
          <el-icon><Wallet /></el-icon>
          <span>Payments</span>
        </el-menu-item>
        <el-menu-item index="/reports" v-if="hasPermission('Reports')">
          <el-icon><Document /></el-icon>
          <span>Reports</span>
        </el-menu-item>
      </el-menu>
    </el-aside>

    <div
      v-if="isMobile && sidebarVisible"
      class="erp-overlay"
      @click="sidebarVisible = false"
    ></div>

    <el-container>
      <el-header class="erp-header">
        <div class="erp-header-left">
          <el-button v-if="isMobile" @click="sidebarVisible = true" link>
            <el-icon :size="24"><Menu /></el-icon>
          </el-button>
          <h2 class="erp-page-title">{{ currentPageTitle }}</h2>
        </div>
        <div class="erp-header-right flex items-center gap-4">
          <!-- <el-button @click="toggleTheme" circle>
            <el-icon v-if="isDark"><Moon /></el-icon>
            <el-icon v-else><Sunny /></el-icon>
          </el-button> -->
          <el-dropdown @command="handleCommand">
            <div class="erp-user-trigger">
              <el-avatar :size="30" class="erp-user-avatar">
                {{ authStore.user?.fullName?.charAt(0) || "U" }}
              </el-avatar>
              <span class="hidden md:inline">{{
                authStore.user?.fullName
              }}</span>
              <el-icon><ArrowDown /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="profile">
                  <el-icon><User /></el-icon>
                  Profile
                </el-dropdown-item>
                <el-dropdown-item command="logout">
                  <el-icon><SwitchButton /></el-icon>
                  Logout
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </el-header>

      <el-main class="erp-main">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, onUnmounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import { ElMessageBox } from "element-plus";
import {
  HomeFilled,
  ArrowDown,
  SwitchButton,
  User,
  Box,
  FolderOpened,
  Coin,
  ScaleToOriginal,
  Document,
  ShoppingCart,
  Money,
  OfficeBuilding,
  RefreshLeft,
  Wallet,
  Switch,
  Menu,
  Close,
  Moon,
  Sunny,
} from "@element-plus/icons-vue";
import { useAuthStore } from "@/stores/auth";
import { useTheme } from "@/composables/useTheme";

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const { isDark, toggleTheme } = useTheme();
const sidebarVisible = ref(false);
const isMobileView = ref(window.innerWidth < 768);

const handleResize = () => {
  const wasMobile = isMobileView.value;
  isMobileView.value = window.innerWidth < 768;
  if (!isMobileView.value && wasMobile) {
    sidebarVisible.value = false;
  }
};

onMounted(() => {
  window.addEventListener("resize", handleResize);
});

onUnmounted(() => {
  window.removeEventListener("resize", handleResize);
});

const isMobile = computed(() => isMobileView.value);

const hasPermission = (key: string) => {
  if (!authStore.user) return true;
  const perms = authStore.user.permissions;
  if (!perms || perms.length === 0) return true;
  return perms.includes(key);
};

const activeMenu = computed(() => route.path);
const currentPageTitle = computed(() => route.name?.toString() || "Dashboard");

const handleMenuSelect = () => {
  if (isMobile.value) {
    sidebarVisible.value = false;
  }
};

const handleCommand = async (command: string) => {
  if (command === "profile") {
    router.push("/profile");
  } else if (command === "logout") {
    try {
      await ElMessageBox.confirm("Are you sure you want to logout?", "Logout", {
        confirmButtonText: "Yes",
        cancelButtonText: "No",
        type: "warning",
      });
      authStore.logout();
      await router.push("/login");
    } catch {
      // User cancelled
    }
  }
};
</script>

<style scoped>
@media (max-width: 768px) {
  ::v-deep(.el-menu-item),
  ::v-deep(.el-sub-menu__title) {
    height: 50px;
    line-height: 50px;
  }
}
</style>
