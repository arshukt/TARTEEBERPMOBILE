<template>
  <div class="erp-page">
    <div class="erp-toolbar">
      <el-input
        v-model="searchTerm"
        placeholder="Search sales by invoice number..."
        prefix-icon="Search"
        class="erp-search"
        clearable
        @input="debouncedSearch"
      />
      <el-button
        type="primary"
        :loading="dialogLoading && !editingSale"
        @click="openDialog()"
      >
        <el-icon><Plus /></el-icon>
        Add Sale
      </el-button>
    </div>

    <el-card>
      <el-table
        :data="saleStore.pagedSales.items"
        v-loading="saleStore.loading"
        stripe
      >
        <el-table-column prop="id" label="ID" width="60" />
        <el-table-column
          prop="invoiceNumber"
          label="Inv No"
          width="120"
        />
        <el-table-column prop="saleDate" label="Date" width="100">
          <template #default="{ row }">
            {{ formatDate(row.saleDate) }}
          </template>
        </el-table-column>
        <el-table-column label="Net Amount" width="120">
          <template #default="{ row }">
            {{ formatCurrency(row.netAmount) }}
          </template>
        </el-table-column>
        <!-- <el-table-column label="Due Amount" width="150">
          <template #default="{ row }">
            <span :class="{ 'text-red-600': row.dueAmount > 0 }">
              {{ formatCurrency(row.dueAmount) }}
            </span>
          </template>
        </el-table-column> -->
        <el-table-column label="Actions" width="130" fixed="right" align="center">
          <template #default="{ row }">
            <el-button link type="primary" @click="openDialog(row)">
              Edit
            </el-button>
            <el-button link type="success" @click="openPrintDialog(row)">
              Print
            </el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination
        v-model:current-page="saleStore.pagedSales.pageNumber"
        v-model:page-size="saleStore.pagedSales.pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="saleStore.pagedSales.totalCount"
        layout="total, sizes, prev, pager, next, jumper"
        @size-change="loadSales"
        @current-change="loadSales"
        class="mt-4"
      />
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="editingSale ? 'Edit Sale' : 'Add Sale'"
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
            <el-form-item label="Customer" prop="customerId">
              <el-select
                v-model="form.customerId"
                placeholder="Select customer (optional)"
                style="width: 100%"
                clearable
                @change="onCustomerChange"
              >
                <el-option
                  v-for="customer in customerStore.customers"
                  :key="customer.id"
                  :label="customer.customerName"
                  :value="customer.id"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Sale Date" prop="saleDate">
              <el-date-picker
                v-model="form.saleDate"
                type="date"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="Invoice Number" prop="invoiceNumber">
          <el-input v-model="form.invoiceNumber" />
        </el-form-item>
        <el-form-item label="Is Credit Sale" prop="isCredit">
          <el-switch v-model="form.isCredit" />
        </el-form-item>
        <el-row :gutter="20" v-if="form.isCredit">
          <el-col :span="12">
            <el-form-item label="Due Date" prop="dueDate">
              <el-date-picker
                v-model="form.dueDate"
                type="date"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Paid Amount" prop="paidAmount">
              <el-input-number
                v-model="form.paidAmount"
                :min="0"
                :precision="2"
                :step="0.01"
                style="width: 100%"
                @change="calculateTotal"
              />
            </el-form-item>
          </el-col>
        </el-row>
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
          <el-table :data="form.saleDetails" border style="width: 100%">
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
                  @change="calculateTotal"
                />
              </template>
            </el-table-column>
            <el-table-column label="Rate" width="140">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.rate"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                  @change="calculateTotal"
                />
              </template>
            </el-table-column>
            <el-table-column label="Discount" width="140">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.discount"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                  @change="calculateTotal"
                />
              </template>
            </el-table-column>
            <el-table-column label="Tax %" width="120">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.taxPercentage"
                  :min="0"
                  :max="100"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                  @change="calculateTotal"
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
            v-for="(row, index) in form.saleDetails"
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
                  @change="calculateTotal"
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
                <span class="mobile-field-label">Rate</span>
                <el-input-number
                  v-model="row.rate"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  style="width: 100%"
                  @change="calculateTotal"
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
                  @change="calculateTotal"
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
                  @change="calculateTotal"
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
      :title="`Print Receipt - ${printSale?.invoiceNumber || ''}`"
      width="480px"
      :show-close="true"
      :close-on-click-modal="false"
      class="sales-print-dialog"
    >
      <div v-if="printSale && company" class="sales-receipt">
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
            <span>{{ printSale.invoiceNumber }}</span>
          </div>
          <div class="receipt-meta-row">
            <span>Date</span>
            <span>{{ formatDate(printSale.saleDate) }}</span>
          </div>
          <div v-if="printCustomer" class="receipt-meta-row">
            <span>Customer</span>
            <span>{{ printCustomer.customerName }}</span>
          </div>
          <div v-if="printCustomer?.mobile" class="receipt-meta-row">
            <span>Mobile</span>
            <span>{{ printCustomer.mobile }}</span>
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
            <tr v-for="detail in printSale.saleDetails" :key="detail.id">
              <td>{{ detail.item?.itemName || ('Item ' + detail.itemId) }}</td>
              <td class="text-right">{{ detail.quantity }}</td>
              <td class="text-right">{{ formatCurrency(detail.rate) }}</td>
              <td class="text-right">{{ formatCurrency(detail.quantity * detail.rate - detail.discount) }}</td>
            </tr>
          </tbody>
        </table>
        <div class="receipt-divider"></div>
        <div class="receipt-totals">
          <div class="receipt-totals-row">
            <span>Subtotal</span>
            <span>{{ formatCurrency(printSale.totalAmount) }}</span>
          </div>
          <div v-if="printSale.discount" class="receipt-totals-row">
            <span>Discount</span>
            <span>{{ formatCurrency(printSale.discount) }}</span>
          </div>
          <div v-if="printSale.taxAmount" class="receipt-totals-row">
            <span>Tax</span>
            <span>{{ formatCurrency(printSale.taxAmount) }}</span>
          </div>
          <div class="receipt-totals-row receipt-totals-total">
            <span>Net</span>
            <span>{{ formatCurrency(printSale.netAmount) }}</span>
          </div>
          <div class="receipt-totals-row">
            <span>Paid</span>
            <span>{{ formatCurrency(printSale.paidAmount) }}</span>
          </div>
          <div v-if="printSale.dueAmount" class="receipt-totals-row">
            <span>Due</span>
            <span class="text-red-600">{{ formatCurrency(printSale.dueAmount) }}</span>
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
import { useSaleStore } from "@/stores/sales";
import { useCustomerStore } from "@/stores/customers";
import { useItemStore } from "@/stores/items";
import { useCompanyStore } from "@/stores/companies";
import { documentNumberService } from "@/services/document-numbers";
import type {
  CreateSale,
  UpdateSale,
  CreateSaleDetail,
  Sale,
} from "@/services/sales";
import { saleService } from "@/services/sales";

const saleStore = useSaleStore();
const customerStore = useCustomerStore();
const itemStore = useItemStore();
const companyStore = useCompanyStore();

const searchTerm = ref("");
const dialogVisible = ref(false);
const editingSale = ref<any>(null);
const formRef = ref<FormInstance>();
const dialogLoading = ref(false);
const saveLoading = ref(false);
const printDialogVisible = ref(false);
const printSale = ref<any>(null);
const company = ref<any>(null);
const printCustomer = ref<any>(null);

const form = ref<CreateSale>({
  customerId: undefined,
  saleDate: new Date().toISOString().split("T")[0],
  invoiceNumber: "",
  saleDetails: [],
  paidAmount: 0,
  dueDate: undefined,
  isCredit: false,
});

const rules: FormRules = {
  saleDate: [
    { required: true, message: "Date is required", trigger: "change" },
  ],
  invoiceNumber: [
    { required: true, message: "Invoice number is required", trigger: "blur" },
  ],
};

const totalAmount = computed(() => {
  return form.value.saleDetails.reduce(
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

const calculateLineTotal = (detail: CreateSaleDetail) => {
  const lineTotal = detail.quantity * detail.rate;
  const afterDiscount = lineTotal - detail.discount;
  const taxAmount = afterDiscount * (detail.taxPercentage / 100);
  return afterDiscount + taxAmount;
};

const calculateTotal = () => {
  // Just a placeholder to trigger computed update
};

let debounceTimer: ReturnType<typeof setTimeout> | null = null;
const debouncedSearch = () => {
  if (debounceTimer) clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    loadSales();
  }, 300);
};

const loadSales = () => {
  saleStore.fetchPaged(
    saleStore.pagedSales.pageNumber,
    saleStore.pagedSales.pageSize,
    searchTerm.value,
  );
};

const loadNextInvoiceNumber = async () => {
  try {
    const response = await documentNumberService.getNext("sale");
    if (response.success && response.data) {
      form.value.invoiceNumber = response.data;
    }
  } catch {
    ElMessage.warning("Failed to load next sale number");
  }
};

const openDialog = async (sale?: any) => {
  editingSale.value = sale || null;
  dialogVisible.value = true;
  dialogLoading.value = true;
  try {
    const tasks = [customerStore.fetchAll(), itemStore.fetchAllWithStock()];

    if (sale) {
      const response = await saleService.getById(sale.id);
      if (response.success && response.data) {
        const data = response.data;
        form.value = {
          customerId: data.customerId,
          saleDate: data.saleDate.split("T")[0],
          invoiceNumber: data.invoiceNumber,
          saleDetails: data.saleDetails.map((d) => ({
            itemId: d.itemId,
            quantity: d.quantity,
            rate: d.rate,
            discount: d.discount,
            taxPercentage: d.taxPercentage,
          })),
          paidAmount: data.paidAmount,
          dueDate: data.dueDate ? data.dueDate.split("T")[0] : undefined,
          isCredit: data.isCredit,
        };
      }
    } else {
      form.value = {
        customerId: undefined,
        saleDate: new Date().toISOString().split("T")[0],
        invoiceNumber: "",
        saleDetails: [],
        paidAmount: 0,
        dueDate: undefined,
        isCredit: false,
      };
      tasks.push(loadNextInvoiceNumber());
    }
    await Promise.all(tasks);
  } finally {
    dialogLoading.value = false;
  }
};

const addItemRow = () => {
  form.value.saleDetails.push({
    itemId: 0,
    quantity: 1,
    rate: 0,
    discount: 0,
    taxPercentage: 0,
  });
};

const removeItemRow = (index: number) => {
  form.value.saleDetails.splice(index, 1);
};

const onItemSelected = (index: number) => {
  const detail = form.value.saleDetails[index];
  const item = itemStore.items.find((i) => i.id === detail.itemId);
  if (item) {
    detail.rate = item.retailRate;
    detail.taxPercentage = item.taxPercentage;
    const stockMsg =
      item.currentStock !== undefined
        ? `Stock: ${item.currentStock}`
        : "Stock: -";
    if (item.currentStock !== undefined && item.currentStock <= 0) {
      ElMessage.warning(`${item.itemName} has no stock available`);
    } else {
      ElMessage.info(`${item.itemName} - ${stockMsg}`);
    }
  }
};

const onCustomerChange = () => {
  if (form.value.customerId) {
    form.value.isCredit = true;
  } else {
    form.value.isCredit = false;
    form.value.paidAmount = 0;
    form.value.dueDate = undefined;
  }
};

const handleSubmit = async () => {
  if (!formRef.value || saveLoading.value) return;
  const valid = await formRef.value.validate().catch(() => false);
  if (!valid) return;

  const hasInvalidItem =
    form.value.saleDetails.length === 0 ||
    form.value.saleDetails.some(
      (detail) => !detail.itemId || detail.quantity <= 0,
    );

  if (hasInvalidItem) {
    ElMessage.warning("Add at least one valid item before saving the sale");
    return;
  }

  const insufficientStock = form.value.saleDetails.some((detail) => {
    const item = itemStore.items.find((i) => i.id === detail.itemId);
    return item && item.currentStock !== undefined && detail.quantity > item.currentStock;
  });

  if (insufficientStock) {
    ElMessage.warning("Cannot save sale: one or more items have insufficient stock");
    return;
  }

  saveLoading.value = true;
  try {
    let success = false;
    if (editingSale.value) {
      success = await saleStore.update({
        id: editingSale.value.id,
        ...form.value,
      } as UpdateSale);
    } else {
      success = await saleStore.create(form.value);
    }
    if (success) {
      dialogVisible.value = false;
      loadSales();
    }
  } finally {
    saveLoading.value = false;
  }
};

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm(
      "Are you sure you want to delete this sale?",
      "Confirm",
      {
        type: "warning",
      },
    );
    const success = await saleStore.remove(id);
    if (success) {
      loadSales();
    }
  } catch {
    // User cancelled
  }
};

const openPrintDialog = async (sale: any) => {
  printDialogVisible.value = true;
  printSale.value = null;
  printCustomer.value = null;
  try {
    const [saleResponse] = await Promise.all([
      saleService.getById(sale.id),
      itemStore.fetchAll(),
    ]);

    if (saleResponse.success && saleResponse.data) {
      const data = saleResponse.data;
      printSale.value = {
        ...data,
        saleDetails: (data.saleDetails || []).map((d: any) => ({
          ...d,
          item: d.item || itemStore.items.find((i) => i.id === d.itemId) || undefined,
        })),
      };
      printCustomer.value = data.customer || null;
    }
  } catch (error: any) {
    ElMessage.error(error?.response?.data?.message || "Failed to load receipt data");
    printDialogVisible.value = false;
  }
};

const printReceipt = () => {
  if (!printSale.value) return;
  const printContent = document.querySelector(".sales-receipt");
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

  const rows = printSale.value.saleDetails
    .map(
      (detail: any) => `<tr>
        <td>${(detail.item?.itemName || "Item " + detail.itemId) || "Item"}</td>
        <td class="text-right">${detail.quantity}</td>
        <td class="text-right">${formatCurrency(detail.rate)}</td>
        <td class="text-right">${formatCurrency(detail.quantity * detail.rate - detail.discount)}</td>
      </tr>`
    )
    .join("");

  printWindow.document.write(`<!doctype html>
<html>
<head>
<title>Receipt - ${printSale.value.invoiceNumber}</title>
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
    <div class="receipt-meta-row"><span>Invoice</span><span>${printSale.value.invoiceNumber}</span></div>
    <div class="receipt-meta-row"><span>Date</span><span>${formatDate(printSale.value.saleDate)}</span></div>
    ${printCustomer.value ? `<div class="receipt-meta-row"><span>Customer</span><span>${printCustomer.value.customerName}</span></div>` : ""}
    ${printCustomer.value?.mobile ? `<div class="receipt-meta-row"><span>Mobile</span><span>${printCustomer.value.mobile}</span></div>` : ""}
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
    <div class="receipt-totals-row"><span>Subtotal</span><span>${formatCurrency(printSale.value.totalAmount)}</span></div>
    ${printSale.value.discount ? `<div class="receipt-totals-row"><span>Discount</span><span>${formatCurrency(printSale.value.discount)}</span></div>` : ""}
    ${printSale.value.taxAmount ? `<div class="receipt-totals-row"><span>Tax</span><span>${formatCurrency(printSale.value.taxAmount)}</span></div>` : ""}
    <div class="receipt-totals-row receipt-totals-total"><span>Net</span><span>${formatCurrency(printSale.value.netAmount)}</span></div>
    <div class="receipt-totals-row"><span>Paid</span><span>${formatCurrency(printSale.value.paidAmount)}</span></div>
    ${printSale.value.dueAmount ? `<div class="receipt-totals-row"><span>Due</span><span class="text-red-600">${formatCurrency(printSale.value.dueAmount)}</span></div>` : ""}
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
  loadSales();
  try {
    await companyStore.fetchFirst();
    company.value = companyStore.currentCompany;
  } catch {}
});
</script>
