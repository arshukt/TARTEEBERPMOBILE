<template>
  <div class="erp-page">
    <div class="erp-toolbar">
      <el-input
        v-model="searchTerm"
        placeholder="Search purchases by invoice number..."
        prefix-icon="Search"
        class="erp-search"
        clearable
        @input="debouncedSearch"
      />
      <el-button
        type="primary"
        :loading="dialogLoading && !editingPurchase"
        @click="openDialog()"
      >
        <el-icon><Plus /></el-icon>
        Add Purchase
      </el-button>
    </div>

    <el-card>
      <el-table
        :data="purchaseStore.pagedPurchases.items"
        v-loading="purchaseStore.loading"
        stripe
      >
        <el-table-column prop="id" label="ID" width="60" />
        <el-table-column
          prop="invoiceNumber"
          label="Inv No"
          width="100"
        />
        <el-table-column prop="purchaseDate" label="Date" width="100">
          <template #default="{ row }">
            {{ formatDate(row.purchaseDate) }}
          </template>
        </el-table-column>
        <el-table-column label="Amount" width="120">
          <template #default="{ row }">
            {{ formatCurrency(row.netAmount) }}
          </template>
        </el-table-column>
        <el-table-column
          label="Actions"
          width="90"
          fixed="right"
          align="center"
        >
          <template #default="{ row }">
            <el-button link type="primary" @click="openDialog(row)">
              Edit
            </el-button>
            <el-button link type="success" @click="openPrintDialog(row)">
              Print
            </el-button>
            <!-- <el-button link type="danger" @click="handleDelete(row)">
              Delete
            </el-button> -->
          </template>
        </el-table-column>
      </el-table>
      <el-pagination
        v-model:current-page="purchaseStore.pagedPurchases.pageNumber"
        v-model:page-size="purchaseStore.pagedPurchases.pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="purchaseStore.pagedPurchases.totalCount"
        layout="total, sizes, prev, pager, next, jumper"
        @size-change="loadPurchases"
        @current-change="loadPurchases"
        class="mt-4"
      />
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="editingPurchase ? 'Edit Purchase' : 'Add Purchase'"
      width="1200px"
      :close-on-click-modal="!saveLoading"
    >
      <el-form
        :model="form"
        :rules="rules"
        ref="formRef"
        label-width="150px"
        class="erp-dialog-form"
        v-loading="dialogLoading"
      >
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="Supplier" prop="supplierId">
              <el-select
                v-model="form.supplierId"
                placeholder="Select supplier"
                style="width: 100%"
              >
                <el-option
                  v-for="supplier in supplierStore.suppliers"
                  :key="supplier.id"
                  :label="supplier.supplierName"
                  :value="supplier.id"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Purchase Date" prop="purchaseDate">
              <el-date-picker
                v-model="form.purchaseDate"
                type="date"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="Invoice Number" prop="invoiceNumber">
          <el-input v-model="form.invoiceNumber" />
        </el-form-item>
        <el-divider content-position="left">Items</el-divider>
        <div class="erp-details-toolbar">
          <el-button type="primary" @click="addItemRow">
            <el-icon><Plus /></el-icon>
            Add Item
          </el-button>
          <div class="erp-total-pill">
            Total:
            <span class="text-green-600">{{
              formatCurrency(totalAmount)
            }}</span>
          </div>
        </div>
        <div class="desktop-line-items">
          <el-table :data="form.purchaseDetails" border style="width: 100%">
            <el-table-column label="Item" width="260">
              <template #default="{ row, $index }">
                <el-select
                  v-model="row.itemId"
                  placeholder="Select item"
                  style="width: 100%"
                  @change="onItemSelected($index)"
                >
                  <el-option
                    v-for="item in itemStore.items"
                    :key="item.id"
                    :label="item.itemName"
                    :value="item.id"
                  />
                </el-select>
              </template>
            </el-table-column>
            <el-table-column label="Stock" width="100">
              <template #default="{ row }">
                <span
                  :class="{
                    'text-red-600':
                      (getItemById(row.itemId)?.currentStock ?? 0) <= 0 &&
                      getItemById(row.itemId)?.currentStock !== undefined,
                  }"
                >
                  {{ getItemById(row.itemId)?.currentStock ?? "-" }}
                </span>
              </template>
            </el-table-column>
            <el-table-column label="Quantity" width="120">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.quantity"
                  :min="1"
                  :step="1"
                  style="width: 100%"
                  @change="calculateTotals"
                />
              </template>
            </el-table-column>
            <el-table-column label="Purchase Rate" width="140">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.purchaseRate"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                  @change="calculateTotals"
                />
              </template>
            </el-table-column>
            <el-table-column label="Cost Rate" width="140">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.costRate"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                />
              </template>
            </el-table-column>
            <el-table-column label="Retail Rate" width="140">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.retailRate"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                />
              </template>
            </el-table-column>
            <el-table-column label="Wholesale Rate" width="160">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.wholesaleRate"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                />
              </template>
            </el-table-column>
            <el-table-column label="MRP" width="120">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.mrp"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                />
              </template>
            </el-table-column>
            <el-table-column label="Discount" width="120">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.discount"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                  @change="calculateTotals"
                />
              </template>
            </el-table-column>
            <el-table-column label="Tax %" width="100">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.taxPercentage"
                  :min="0"
                  :max="100"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                  @change="calculateTotals"
                />
              </template>
            </el-table-column>
            <el-table-column label="Batch Number" width="150">
              <template #default="{ row }">
                <el-input v-model="row.batchNumber" />
              </template>
            </el-table-column>
            <el-table-column label="Expiry Date" width="160">
              <template #default="{ row }">
                <el-date-picker
                  v-model="row.expiryDate"
                  type="date"
                  style="width: 100%"
                />
              </template>
            </el-table-column>
            <el-table-column label="Total" width="140">
              <template #default="{ row }">
                <span class="font-semibold">{{
                  formatCurrency(calculateLineTotal(row))
                }}</span>
              </template>
            </el-table-column>
            <el-table-column label="Actions" width="80">
              <template #default="{ $index }">
                <el-button
                  type="danger"
                  size="small"
                  link
                  @click="removeItemRow($index)"
                >
                  Delete
                </el-button>
              </template>
            </el-table-column>
          </el-table>
        </div>
        <div class="mobile-line-items">
          <div
            v-for="(row, index) in form.purchaseDetails"
            :key="index"
            class="mobile-line-item"
          >
            <div class="mobile-line-item-header">
              <span class="mobile-line-item-title">Item {{ index + 1 }}</span>
              <el-button
                type="danger"
                size="small"
                link
                @click="removeItemRow(index)"
              >
                Delete
              </el-button>
            </div>
            <div class="mobile-line-grid">
              <label class="full">
                <span class="mobile-field-label">Item</span>
                <el-select
                  v-model="row.itemId"
                  placeholder="Select item"
                  style="width: 100%"
                  @change="onItemSelected(index)"
                >
                  <el-option
                    v-for="item in itemStore.items"
                    :key="item.id"
                    :label="item.itemName"
                    :value="item.id"
                  />
                </el-select>
              </label>
              <label>
                <span class="mobile-field-label">Quantity</span>
                <el-input-number
                  v-model="row.quantity"
                  :min="1"
                  :step="1"
                  style="width: 100%"
                  @change="calculateTotals"
                />
              </label>
              <label>
                <span class="mobile-field-label">Stock</span>
                <span
                  :class="{
                    'text-red-600':
                      (getItemById(row.itemId)?.currentStock ?? 0) <= 0 &&
                      getItemById(row.itemId)?.currentStock !== undefined,
                  }"
                >
                  {{ getItemById(row.itemId)?.currentStock ?? "-" }}
                </span>
              </label>
              <label>
                <span class="mobile-field-label">Purchase Rate</span>
                <el-input-number
                  v-model="row.purchaseRate"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                  @change="calculateTotals"
                />
              </label>
              <label>
                <span class="mobile-field-label">Cost Rate</span>
                <el-input-number
                  v-model="row.costRate"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                />
              </label>
              <label>
                <span class="mobile-field-label">Retail Rate</span>
                <el-input-number
                  v-model="row.retailRate"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                />
              </label>
              <label>
                <span class="mobile-field-label">Wholesale Rate</span>
                <el-input-number
                  v-model="row.wholesaleRate"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                />
              </label>
              <label>
                <span class="mobile-field-label">MRP</span>
                <el-input-number
                  v-model="row.mrp"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                />
              </label>
              <label>
                <span class="mobile-field-label">Discount</span>
                <el-input-number
                  v-model="row.discount"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                  @change="calculateTotals"
                />
              </label>
              <label>
                <span class="mobile-field-label">Tax %</span>
                <el-input-number
                  v-model="row.taxPercentage"
                  :min="0"
                  :max="100"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                  @change="calculateTotals"
                />
              </label>
              <label>
                <span class="mobile-field-label">Batch Number</span>
                <el-input v-model="row.batchNumber" />
              </label>
              <label>
                <span class="mobile-field-label">Expiry Date</span>
                <el-date-picker
                  v-model="row.expiryDate"
                  type="date"
                  style="width: 100%"
                />
              </label>
            </div>
            <div class="mobile-line-total">
              <span>Total</span>
              <span>{{ formatCurrency(calculateLineTotal(row)) }}</span>
            </div>
          </div>
        </div>
      </el-form>

      <template #footer>
        <div class="dialog-footer">
          <el-button :disabled="saveLoading" @click="dialogVisible = false"
            >Cancel</el-button
          >
          <el-button
            type="primary"
            :loading="saveLoading"
            :disabled="dialogLoading"
            @click="handleSubmit"
            >Save</el-button
          >
        </div>
      </template>
    </el-dialog>

    <el-dialog
      v-model="printDialogVisible"
      :title="`Print - ${printPurchase?.invoiceNumber || ''}`"
      width="480px"
      :show-close="true"
      :close-on-click-modal="false"
      class="purchase-print-dialog"
    >
      <div v-if="printPurchase && company" class="purchase-receipt">
        <div class="receipt-header">
          <h2 class="receipt-company">{{ company.companyName }}</h2>
          <p v-if="company.address">{{ company.address }}</p>
          <p v-if="company.phone">Tel: {{ company.phone }}</p>
          <p v-if="company.mobile">Mob: {{ company.mobile }}</p>
          <p v-if="company.email">Email: {{ company.email }}</p>
          <p v-if="company.taxNumber">Tax No: {{ company.taxNumber }}</p>
        </div>
        <div class="receipt-divider"></div>
        <div class="receipt-meta">
          <div class="receipt-meta-row">
            <span>Invoice</span>
            <span>{{ printPurchase.invoiceNumber }}</span>
          </div>
          <div class="receipt-meta-row">
            <span>Date</span>
            <span>{{ formatDate(printPurchase.purchaseDate) }}</span>
          </div>
          <div v-if="printSupplier" class="receipt-meta-row">
            <span>Supplier</span>
            <span>{{ printSupplier.supplierName }}</span>
          </div>
          <div v-if="printSupplier?.mobile" class="receipt-meta-row">
            <span>Mobile</span>
            <span>{{ printSupplier.mobile }}</span>
          </div>
        </div>
        <div class="receipt-divider"></div>
        <table class="receipt-table">
          <thead>
            <tr>
              <th>Item</th>
              <th class="text-right">Qty</th>
              <th class="text-right">Rate</th>
              <th class="text-right">Total</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="detail in printPurchase.purchaseDetails" :key="detail.id">
              <td>{{ detail.item?.itemName || ('Item ' + detail.itemId) }}</td>
              <td class="text-right">{{ detail.quantity }}</td>
              <td class="text-right">{{ formatCurrency(detail.purchaseRate) }}</td>
              <td class="text-right">{{ formatCurrency(detail.quantity * detail.purchaseRate - detail.discount) }}</td>
            </tr>
          </tbody>
        </table>
        <div class="receipt-divider"></div>
        <div class="receipt-totals">
          <div class="receipt-totals-row">
            <span>Subtotal</span>
            <span>{{ formatCurrency(printPurchase.totalAmount) }}</span>
          </div>
          <div v-if="printPurchase.discount" class="receipt-totals-row">
            <span>Discount</span>
            <span>{{ formatCurrency(printPurchase.discount) }}</span>
          </div>
          <div v-if="printPurchase.taxAmount" class="receipt-totals-row">
            <span>Tax</span>
            <span>{{ formatCurrency(printPurchase.taxAmount) }}</span>
          </div>
          <div class="receipt-totals-row receipt-totals-total">
            <span>Net</span>
            <span>{{ formatCurrency(printPurchase.netAmount) }}</span>
          </div>
        </div>
        <div class="receipt-divider"></div>
        <div class="receipt-footer">
          <p>Thank you for your business!</p>
        </div>
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="printDialogVisible = false">Close</el-button>
          <el-button type="primary" @click="printReceipt">Print</el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import {
  ElMessage,
  ElMessageBox,
  type FormInstance,
  type FormRules,
} from "element-plus";
import { Plus } from "@element-plus/icons-vue";
import { usePurchaseStore } from "@/stores/purchases";
import { useSupplierStore } from "@/stores/suppliers";
import { useItemStore } from "@/stores/items";
import { useCompanyStore } from "@/stores/companies";
import { documentNumberService } from "@/services/document-numbers";
import type {
  CreatePurchase,
  UpdatePurchase,
  CreatePurchaseDetail,
} from "@/services/purchases";
import { purchaseService } from "@/services/purchases";

const purchaseStore = usePurchaseStore();
const supplierStore = useSupplierStore();
const itemStore = useItemStore();
const companyStore = useCompanyStore();

const searchTerm = ref("");
const dialogVisible = ref(false);
const editingPurchase = ref<any>(null);
const formRef = ref<FormInstance>();
const dialogLoading = ref(false);
const saveLoading = ref(false);
const printDialogVisible = ref(false);
const printPurchase = ref<any>(null);
const company = ref<any>(null);
const printSupplier = ref<any>(null);

const form = ref<CreatePurchase>({
  supplierId: 0,
  purchaseDate: new Date().toISOString().split("T")[0],
  invoiceNumber: "",
  purchaseDetails: [],
});

const rules: FormRules = {
  supplierId: [
    { required: true, message: "Supplier is required", trigger: "change" },
  ],
  purchaseDate: [
    { required: true, message: "Date is required", trigger: "change" },
  ],
  invoiceNumber: [
    { required: true, message: "Invoice number is required", trigger: "blur" },
  ],
};

const totalAmount = computed(() => {
  return form.value.purchaseDetails.reduce(
    (sum, detail) => sum + calculateLineTotal(detail),
    0,
  );
});

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString();
};

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "QAR",
  }).format(amount);
};

const getItemById = (itemId: number) => {
  return itemStore.items.find((i) => i.id === itemId);
};

const calculateLineTotal = (detail: CreatePurchaseDetail) => {
  const lineTotal = detail.quantity * detail.purchaseRate;
  const afterDiscount = lineTotal - detail.discount;
  const taxAmount = afterDiscount * (detail.taxPercentage / 100);
  return afterDiscount + taxAmount;
};

const calculateTotals = () => {
  // Just a placeholder to trigger computed update
};

let debounceTimer: ReturnType<typeof setTimeout> | null = null;
const debouncedSearch = () => {
  if (debounceTimer) clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    loadPurchases();
  }, 300);
};

const loadPurchases = () => {
  purchaseStore.fetchPaged(
    purchaseStore.pagedPurchases.pageNumber,
    purchaseStore.pagedPurchases.pageSize,
    searchTerm.value,
  );
};

const loadNextInvoiceNumber = async () => {
  try {
    const response = await documentNumberService.getNext("purchase");
    if (response.success && response.data) {
      form.value.invoiceNumber = response.data;
    }
  } catch {
    ElMessage.warning("Failed to load next purchase number");
  }
};

const openDialog = async (purchase?: any) => {
  editingPurchase.value = purchase || null;
  dialogVisible.value = true;
  dialogLoading.value = true;
  try {
    const tasks = [supplierStore.fetchAll(), itemStore.fetchAllWithStock()];

    if (purchase) {
      const response = await purchaseService.getById(purchase.id);
      if (response.success && response.data) {
        const data = response.data;
        form.value = {
          supplierId: data.supplierId,
          purchaseDate: data.purchaseDate.split("T")[0],
          invoiceNumber: data.invoiceNumber,
          purchaseDetails: data.purchaseDetails.map((d) => ({
            itemId: d.itemId,
            quantity: d.quantity,
            purchaseRate: d.purchaseRate,
            costRate: d.costRate,
            retailRate: d.retailRate,
            wholesaleRate: d.wholesaleRate,
            mrp: d.mrp,
            discount: d.discount,
            taxPercentage: d.taxPercentage,
            batchNumber: d.batchNumber,
            expiryDate: d.expiryDate ? d.expiryDate.split("T")[0] : undefined,
          })),
        };
      }
    } else {
      form.value = {
        supplierId: 0,
        purchaseDate: new Date().toISOString().split("T")[0],
        invoiceNumber: "",
        purchaseDetails: [],
      };
      tasks.push(loadNextInvoiceNumber());
    }
    await Promise.all(tasks);
  } finally {
    dialogLoading.value = false;
  }
};

const addItemRow = () => {
  form.value.purchaseDetails.push({
    itemId: 0,
    quantity: 1,
    purchaseRate: 0,
    costRate: 0,
    retailRate: 0,
    wholesaleRate: 0,
    mrp: 0,
    discount: 0,
    taxPercentage: 0,
    batchNumber: "",
    expiryDate: "",
  });
};

const removeItemRow = (index: number) => {
  form.value.purchaseDetails.splice(index, 1);
};

const onItemSelected = (index: number) => {
  const detail = form.value.purchaseDetails[index];
  const item = itemStore.items.find((i) => i.id === detail.itemId);
  if (item) {
    detail.purchaseRate = item.purchaseRate;
    detail.costRate = item.costRate;
    detail.retailRate = item.retailRate;
    detail.wholesaleRate = item.wholesaleRate;
    detail.mrp = item.mrp;
    detail.taxPercentage = item.taxPercentage;
    const stockMsg =
      item.currentStock !== undefined
        ? `Stock: ${item.currentStock}`
        : "Stock: -";
    ElMessage.info(`${item.itemName} - ${stockMsg}`);
  }
};

const handleSubmit = async () => {
  if (!formRef.value || saveLoading.value) return;
  const valid = await formRef.value.validate().catch(() => false);
  if (!valid) return;

  const hasInvalidItem =
    form.value.purchaseDetails.length === 0 ||
    form.value.purchaseDetails.some(
      (detail) => !detail.itemId || detail.quantity <= 0,
    );

  if (hasInvalidItem) {
    ElMessage.warning("Add at least one valid item before saving the purchase");
    return;
  }

  saveLoading.value = true;
  try {
    let success = false;
    if (editingPurchase.value) {
      success = await purchaseStore.update({
        id: editingPurchase.value.id,
        ...form.value,
      } as UpdatePurchase);
    } else {
      success = await purchaseStore.create(form.value);
    }
    if (success) {
      dialogVisible.value = false;
      loadPurchases();
    }
  } finally {
    saveLoading.value = false;
  }
};

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm(
      "Are you sure you want to delete this purchase?",
      "Confirm",
      {
        type: "warning",
      },
    );
    const success = await purchaseStore.remove(id);
    if (success) {
      loadPurchases();
    }
  } catch {
    // User cancelled
  }
};

const openPrintDialog = async (purchase: any) => {
  printDialogVisible.value = true;
  printPurchase.value = null;
  printSupplier.value = null;
  try {
    const [response] = await Promise.all([
      purchaseService.getById(purchase.id),
      itemStore.fetchAll(),
    ]);

    if (response.success && response.data) {
      const data = response.data;
      printPurchase.value = {
        ...data,
        purchaseDetails: (data.purchaseDetails || []).map((d: any) => ({
          ...d,
          item: d.item || itemStore.items.find((i) => i.id === d.itemId) || undefined,
        })),
      };
      printSupplier.value = data.supplier || null;
    }
  } catch (error: any) {
    ElMessage.error(error?.response?.data?.message || "Failed to load receipt data");
    printDialogVisible.value = false;
  }
};

const printReceipt = () => {
  if (!printPurchase.value) return;
  const printContent = document.querySelector(".purchase-receipt");
  if (!printContent) return;

  const printWindow = window.open("", "_blank", "width=320,height=600");
  if (!printWindow) {
    ElMessage.error("Please allow popups to print");
    return;
  }

  const companyName = company.value?.companyName || "Company";
  const companyAddress = company.value?.address || "";
  const companyPhone = company.value?.phone || "";
  const companyMobile = company.value?.mobile || "";
  const companyEmail = company.value?.email || "";
  const companyTax = company.value?.taxNumber || "";

  const rows = printPurchase.value.purchaseDetails
    .map(
      (detail: any) => `<tr>
        <td>${(detail.item?.itemName || "Item " + detail.itemId) || "Item"}</td>
        <td class="text-right">${detail.quantity}</td>
        <td class="text-right">${formatCurrency(detail.purchaseRate)}</td>
        <td class="text-right">${formatCurrency(detail.quantity * detail.purchaseRate - detail.discount)}</td>
      </tr>`
    )
    .join("");

  printWindow.document.write(`<!doctype html>
<html>
<head>
<title>Purchase - ${printPurchase.value.invoiceNumber}</title>
<style>
  * { box-sizing: border-box; }
  body {
    margin: 0;
    padding: 12px;
    font-family: monospace;
    font-size: 12px;
    line-height: 1.35;
    color: #000;
  }
  .receipt-header, .receipt-meta, .receipt-totals, .receipt-footer {
    text-align: center;
  }
  .receipt-header h1 {
    margin: 0 0 6px;
    font-size: 16px;
    font-weight: 700;
  }
  .receipt-header p {
    margin: 2px 0;
  }
  .receipt-divider {
    border-top: 1px dashed #000;
    margin: 8px 0;
  }
  .receipt-meta-row {
    display: flex;
    justify-content: space-between;
    gap: 8px;
    text-align: left;
  }
  .receipt-table {
    width: 100%;
    border-collapse: collapse;
  }
  .receipt-table th, .receipt-table td {
    padding: 4px 2px;
    text-align: left;
    border-bottom: 1px dotted #000;
    vertical-align: top;
  }
  .receipt-table th {
    border-bottom: 1px solid #000;
  }
  .receipt-table .text-right {
    text-align: right;
  }
  .receipt-totals-row {
    display: flex;
    justify-content: space-between;
    gap: 8px;
  }
  .receipt-totals-total {
    font-weight: 700;
    border-top: 1px solid #000;
    border-bottom: 1px solid #000;
    padding: 4px 0;
    margin: 4px 0;
  }
  .text-red-600 { color: #b00000; }
  .text-right { text-align: right; }
  @media print {
    body { padding: 0; }
  }
</style>
</head>
<body>
  <div class="receipt-header">
    <h1>${companyName}</h1>
    ${companyAddress ? `<p>${companyAddress}</p>` : ""}
    ${companyPhone ? `<p>Tel: ${companyPhone}</p>` : ""}
    ${companyMobile ? `<p>Mob: ${companyMobile}</p>` : ""}
    ${companyEmail ? `<p>${companyEmail}</p>` : ""}
    ${companyTax ? `<p>Tax No: ${companyTax}</p>` : ""}
  </div>
  <div class="receipt-divider"></div>
  <div class="receipt-meta">
    <div class="receipt-meta-row"><span>Invoice</span><span>${printPurchase.value.invoiceNumber}</span></div>
    <div class="receipt-meta-row"><span>Date</span><span>${formatDate(printPurchase.value.purchaseDate)}</span></div>
    ${printSupplier.value ? `<div class="receipt-meta-row"><span>Supplier</span><span>${printSupplier.value.supplierName}</span></div>` : ""}
    ${printSupplier.value?.mobile ? `<div class="receipt-meta-row"><span>Mobile</span><span>${printSupplier.value.mobile}</span></div>` : ""}
  </div>
  <div class="receipt-divider"></div>
  <table class="receipt-table">
    <thead>
      <tr>
        <th>Item</th>
        <th class="text-right">Qty</th>
        <th class="text-right">Rate</th>
        <th class="text-right">Total</th>
      </tr>
    </thead>
    <tbody>${rows}</tbody>
  </table>
  <div class="receipt-divider"></div>
  <div class="receipt-totals">
    <div class="receipt-totals-row"><span>Subtotal</span><span>${formatCurrency(printPurchase.value.totalAmount)}</span></div>
    ${printPurchase.value.discount ? `<div class="receipt-totals-row"><span>Discount</span><span>${formatCurrency(printPurchase.value.discount)}</span></div>` : ""}
    ${printPurchase.value.taxAmount ? `<div class="receipt-totals-row"><span>Tax</span><span>${formatCurrency(printPurchase.value.taxAmount)}</span></div>` : ""}
    <div class="receipt-totals-row receipt-totals-total"><span>Net</span><span>${formatCurrency(printPurchase.value.netAmount)}</span></div>
  </div>
  <div class="receipt-divider"></div>
  <div class="receipt-footer">
    <p>Thank you for your business!</p>
  </div>
</body>
</html>`);
  printWindow.document.close();
  printWindow.focus();
  printWindow.print();
};

onMounted(async () => {
  loadPurchases();
  try {
    await companyStore.fetchFirst();
    company.value = companyStore.currentCompany;
  } catch {}
});
</script>
