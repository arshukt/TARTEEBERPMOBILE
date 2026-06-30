import { createRouter, createWebHistory } from "vue-router";
import { useAuthStore } from "@/stores/auth";
import type { RouteRecordRaw } from "vue-router";

const routes: RouteRecordRaw[] = [
  {
    path: "/login",
    name: "Login",
    component: () => import("@/pages/LoginPage.vue"),
    meta: { requiresGuest: true },
  },
  {
    path: "/",
    name: "Layout",
    component: () => import("@/layouts/MainLayout.vue"),
    meta: { requiresAuth: true },
    children: [
      {
        path: "",
        name: "Dashboard",
        component: () => import("@/pages/DashboardPage.vue"),
      },
      {
        path: "/profile",
        name: "Profile",
        component: () => import("@/pages/UserProfilePage.vue"),
      },
      {
        path: "/roles",
        name: "Roles",
        component: () => import("@/pages/RolesPage.vue"),
        meta: { permission: "Roles" },
      },
      {
        path: "/users",
        name: "Users",
        component: () => import("@/pages/UsersPage.vue"),
        meta: { permission: "Users" },
      },
      {
        path: "/company",
        name: "Company",
        component: () => import("@/pages/CompanyPage.vue"),
      },
      {
        path: "/customers",
        name: "Customers",
        component: () => import("@/pages/CustomersPage.vue"),
      },
      {
        path: "/suppliers",
        name: "Suppliers",
        component: () => import("@/pages/SuppliersPage.vue"),
      },
      {
        path: "/categories",
        name: "Categories",
        component: () => import("@/pages/CategoriesPage.vue"),
      },
      {
        path: "/brands",
        name: "Brands",
        component: () => import("@/pages/BrandsPage.vue"),
      },
      {
        path: "/units",
        name: "Units",
        component: () => import("@/pages/UnitsPage.vue"),
      },
      {
        path: "/items",
        name: "Items",
        component: () => import("@/pages/ItemsPage.vue"),
      },
      {
        path: "/opening-stocks",
        name: "OpeningStocks",
        component: () => import("@/pages/OpeningStocksPage.vue"),
      },
      {
        path: "/purchases",
        name: "Purchases",
        component: () => import("@/pages/PurchasesPage.vue"),
      },
      {
        path: "/purchase-returns",
        name: "PurchaseReturns",
        component: () => import("@/pages/PurchaseReturnsPage.vue"),
      },
      {
        path: "/sales",
        name: "Sales",
        component: () => import("@/pages/SalesPage.vue"),
      },
      {
        path: "/sale-returns",
        name: "SaleReturns",
        component: () => import("@/pages/SaleReturnsPage.vue"),
      },
      {
        path: "/stock-adjustments",
        name: "StockAdjustments",
        component: () => import("@/pages/StockAdjustmentsPage.vue"),
      },
      {
        path: "/stock-transfers",
        name: "StockTransfers",
        component: () => import("@/pages/StockTransfersPage.vue"),
      },
      {
        path: "/stock-ledger",
        name: "StockLedger",
        component: () => import("@/pages/StockLedgerPage.vue"),
      },
      {
        path: "/payments",
        name: "Payments",
        component: () => import("@/pages/PaymentsPage.vue"),
      },
      {
        path: "/reports",
        name: "Reports",
        component: () => import("@/pages/ReportsPage.vue"),
      },
    ],
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore();

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next("/login");
  } else if (to.meta.requiresGuest && authStore.isAuthenticated) {
    next("/");
  } else {
    if (authStore.isAuthenticated && !authStore.user) {
      await authStore.fetchCurrentUser();
    }

    // Check permissions
    if (to.meta.permission && authStore.user) {
      const permissions = authStore.user.permissions;
      const hasPermission =
        authStore.user.roleName === "Admin" ||
        !permissions ||
        permissions.length === 0 ||
        permissions.includes(to.meta.permission as string);

      if (!hasPermission) {
        return next("/"); // Redirect to dashboard if no permission
      }
    }

    next();
  }
});

export default router;
