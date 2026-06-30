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
            <el-button type="primary" :loading="dialogLoading && !editingSale" @click="openDialog()">
                <el-icon><Plus /></el-icon>
                Add Sale
            </el-button>
        </div>

        <el-card>
            <el-table :data="saleStore.pagedSales.items" v-loading="saleStore.loading" stripe>
                <el-table-column prop="id" label="ID" width="80" />
                <el-table-column prop="invoiceNumber" label="Invoice Number" width="180" />
                <el-table-column prop="saleDate" label="Date" width="150">
                    <template #default="{ row }">
                        {{ formatDate(row.saleDate) }}
                    </template>
                </el-table-column>
                <el-table-column label="Net Amount" width="150">
                    <template #default="{ row }">
                        {{ formatCurrency(row.netAmount) }}
                    </template>
                </el-table-column>
                <el-table-column label="Due Amount" width="150">
                    <template #default="{ row }">
                        <span :class="{ 'text-red-600': row.dueAmount > 0 }">
                            {{ formatCurrency(row.dueAmount) }}
                        </span>
                    </template>
                </el-table-column>
                <el-table-column label="Actions" width="200">
                    <template #default="{ row }">
                        <el-button type="primary" size="small" link @click="openDialog(row)">
                            Edit
                        </el-button>
                        <el-button type="danger" size="small" link @click="handleDelete(row.id)">
                            Delete
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
            <el-form :model="form" :rules="rules" ref="formRef" label-width="150px" class="erp-dialog-form" v-loading="dialogLoading">
                <el-row :gutter="20">
                    <el-col :span="12">
                        <el-form-item label="Customer" prop="customerId">
                            <el-select v-model="form.customerId" placeholder="Select customer (optional)" style="width: 100%" clearable @change="onCustomerChange">
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
                            <el-date-picker v-model="form.saleDate" type="date" style="width: 100%" />
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
                            <el-date-picker v-model="form.dueDate" type="date" style="width: 100%" />
                        </el-form-item>
                    </el-col>
                    <el-col :span="12">
                        <el-form-item label="Paid Amount" prop="paidAmount">
                            <el-input-number v-model="form.paidAmount" :min="0" :precision="2" :step="0.01" style="width: 100%" @change="calculateTotal" />
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
                        Total: <span class="text-green-600">{{ formatCurrency(totalAmount) }}</span>
                    </div>
                </div>
                <div class="desktop-line-items">
                <el-table :data="form.saleDetails" border style="width: 100%">
                    <el-table-column label="Item" width="250">
                        <template #default="{ row, $index }">
                            <el-select v-model="row.itemId" placeholder="Select item" style="width: 100%" @change="onItemSelected($index)">
                                <el-option
                                    v-for="item in itemStore.items"
                                    :key="item.id"
                                    :label="item.itemName"
                                    :value="item.id"
                                />
                            </el-select>
                        </template>
                    </el-table-column>
                    <el-table-column label="Quantity" width="120">
                        <template #default="{ row }">
                            <el-input-number v-model="row.quantity" :min="1" :step="1" style="width: 100%" @change="calculateTotal" />
                        </template>
                    </el-table-column>
                    <el-table-column label="Rate" width="140">
                        <template #default="{ row }">
                            <el-input-number v-model="row.rate" :min="0" :precision="2" :step="0.01" style="width: 100%" @change="calculateTotal" />
                        </template>
                    </el-table-column>
                    <el-table-column label="Discount" width="140">
                        <template #default="{ row }">
                            <el-input-number v-model="row.discount" :min="0" :precision="2" :step="0.01" style="width: 100%" @change="calculateTotal" />
                        </template>
                    </el-table-column>
                    <el-table-column label="Tax %" width="120">
                        <template #default="{ row }">
                            <el-input-number v-model="row.taxPercentage" :min="0" :max="100" :precision="2" :step="0.01" style="width: 100%" @change="calculateTotal" />
                        </template>
                    </el-table-column>
                    <el-table-column label="Total" width="140">
                        <template #default="{ row }">
                            <span class="font-semibold">{{ formatCurrency(calculateLineTotal(row)) }}</span>
                        </template>
                    </el-table-column>
                    <el-table-column label="Actions" width="80">
                        <template #default="{ $index }">
                            <el-button type="danger" size="small" link @click="removeItemRow($index)">
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
                            <el-button type="danger" size="small" link @click="removeItemRow(index)">
                                Delete
                            </el-button>
                        </div>
                        <div class="mobile-line-grid">
                            <label class="full">
                                <span class="mobile-field-label">Item</span>
                                <el-select v-model="row.itemId" placeholder="Select item" style="width: 100%" @change="onItemSelected(index)">
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
                                <el-input-number v-model="row.quantity" :min="1" :step="1" style="width: 100%" @change="calculateTotal" />
                            </label>
                            <label>
                                <span class="mobile-field-label">Rate</span>
                                <el-input-number v-model="row.rate" :min="0" :precision="2" :step="0.01" style="width: 100%" @change="calculateTotal" />
                            </label>
                            <label>
                                <span class="mobile-field-label">Discount</span>
                                <el-input-number v-model="row.discount" :min="0" :precision="2" :step="0.01" style="width: 100%" @change="calculateTotal" />
                            </label>
                            <label>
                                <span class="mobile-field-label">Tax %</span>
                                <el-input-number v-model="row.taxPercentage" :min="0" :max="100" :precision="2" :step="0.01" style="width: 100%" @change="calculateTotal" />
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
                    <el-button :disabled="saveLoading" @click="dialogVisible = false">Cancel</el-button>
                    <el-button type="primary" :loading="saveLoading" :disabled="dialogLoading" @click="handleSubmit">Save</el-button>
                </div>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { ElMessage, ElMessageBox, type FormInstance, type FormRules } from "element-plus";
import { Plus } from "@element-plus/icons-vue";
import { useSaleStore } from "@/stores/sales";
import { useCustomerStore } from "@/stores/customers";
import { useItemStore } from "@/stores/items";
import { documentNumberService } from "@/services/document-numbers";
import type { CreateSale, UpdateSale, CreateSaleDetail, Sale } from "@/services/sales";

const saleStore = useSaleStore();
const customerStore = useCustomerStore();
const itemStore = useItemStore();

const searchTerm = ref("");
const dialogVisible = ref(false);
const editingSale = ref<any>(null);
const formRef = ref<FormInstance>();
const dialogLoading = ref(false);
const saveLoading = ref(false);

const form = ref<CreateSale>({
    customerId: undefined,
    saleDate: new Date().toISOString().split("T")[0],
    invoiceNumber: "",
    saleDetails: [],
    paidAmount: 0,
    dueDate: undefined,
    isCredit: false
});

const rules: FormRules = {
    saleDate: [{ required: true, message: "Date is required", trigger: "change" }],
    invoiceNumber: [{ required: true, message: "Invoice number is required", trigger: "blur" }]
};

const totalAmount = computed(() => {
    return form.value.saleDetails.reduce((sum, detail) => sum + calculateLineTotal(detail), 0);
});

const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleDateString();
};

const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD"
    }).format(amount);
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
        searchTerm.value
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
    if (sale) {
        form.value = {
            customerId: sale.customerId,
            saleDate: sale.saleDate.split("T")[0],
            invoiceNumber: sale.invoiceNumber,
            saleDetails: sale.saleDetails,
            paidAmount: sale.paidAmount,
            dueDate: sale.dueDate ? sale.dueDate.split("T")[0] : undefined,
            isCredit: sale.isCredit
        };
    } else {
        form.value = {
            customerId: undefined,
            saleDate: new Date().toISOString().split("T")[0],
            invoiceNumber: "",
            saleDetails: [],
            paidAmount: 0,
            dueDate: undefined,
            isCredit: false
        };
    }
    dialogVisible.value = true;
    dialogLoading.value = true;
    try {
        const tasks = [
            customerStore.fetchAll(),
            itemStore.fetchAll()
        ];
        if (!sale) {
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
        taxPercentage: 0
    });
};

const removeItemRow = (index: number) => {
    form.value.saleDetails.splice(index, 1);
};

const onItemSelected = (index: number) => {
    const detail = form.value.saleDetails[index];
    const item = itemStore.items.find(i => i.id === detail.itemId);
    if (item) {
        detail.rate = item.retailRate;
        detail.taxPercentage = item.taxPercentage;
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

    const hasInvalidItem = form.value.saleDetails.length === 0
        || form.value.saleDetails.some(detail => !detail.itemId || detail.quantity <= 0);

    if (hasInvalidItem) {
        ElMessage.warning("Add at least one valid item before saving the sale");
        return;
    }

    saveLoading.value = true;
    try {
        let success = false;
        if (editingSale.value) {
            success = await saleStore.update({
                id: editingSale.value.id,
                ...form.value
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
        await ElMessageBox.confirm("Are you sure you want to delete this sale?", "Confirm", {
            type: "warning"
        });
        const success = await saleStore.remove(id);
        if (success) {
            loadSales();
        }
    } catch {
        // User cancelled
    }
};

onMounted(() => {
    loadSales();
});
</script>
