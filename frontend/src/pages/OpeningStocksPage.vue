<template>
    <div class="erp-page">
        <div class="erp-toolbar">
            <el-input
                v-model="searchTerm"
                placeholder="Search opening stocks..."
                prefix-icon="Search"
                class="erp-search"
                clearable
                @input="debouncedSearch"
            />
            <el-button type="primary" :loading="dialogLoading && !editingOpeningStock" @click="openDialog()">
                <el-icon><Plus /></el-icon>
                Add Opening Stock
            </el-button>
        </div>

        <el-card>
            <el-table :data="openingStockStore.pagedOpeningStocks.items" v-loading="openingStockStore.loading" stripe>
                <el-table-column prop="id" label="ID" width="80" />
                <el-table-column prop="date" label="Date" width="150">
                    <template #default="{ row }">
                        {{ formatDate(row.date) }}
                    </template>
                </el-table-column>
                <el-table-column prop="notes" label="Notes" />
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
                v-model:current-page="openingStockStore.pagedOpeningStocks.pageNumber"
                v-model:page-size="openingStockStore.pagedOpeningStocks.pageSize"
                :page-sizes="[10, 20, 50, 100]"
                :total="openingStockStore.pagedOpeningStocks.totalCount"
                layout="total, sizes, prev, pager, next, jumper"
                @size-change="loadOpeningStocks"
                @current-change="loadOpeningStocks"
                class="mt-4"
            />
        </el-card>

        <el-dialog
            v-model="dialogVisible"
            :title="editingOpeningStock ? 'Edit Opening Stock' : 'Add Opening Stock'"
            width="1000px"
            :close-on-click-modal="!saveLoading"
        >
            <el-form :model="form" :rules="rules" ref="formRef" label-width="150px" class="erp-dialog-form" v-loading="dialogLoading">
                <el-form-item label="Date" prop="date">
                    <el-date-picker v-model="form.date" type="date" style="width: 100%" />
                </el-form-item>
                <el-form-item label="Notes" prop="notes">
                    <el-input v-model="form.notes" type="textarea" :rows="2" />
                </el-form-item>
                <el-divider content-position="left">Items</el-divider>
                <div class="erp-details-toolbar">
                    <el-button type="primary" @click="addItemRow">
                        <el-icon><Plus /></el-icon>
                        Add Item
                    </el-button>
                </div>
                <div class="desktop-line-items">
                <el-table :data="form.openingStockDetails" border style="width: 100%">
                    <el-table-column label="Item" width="300">
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
                    <el-table-column label="Quantity" width="150">
                        <template #default="{ row }">
                            <el-input-number v-model="row.quantity" :min="0" :step="1" style="width: 100%" />
                        </template>
                    </el-table-column>
                    <el-table-column label="Purchase Rate" width="150">
                        <template #default="{ row }">
                            <el-input-number v-model="row.purchaseRate" :min="0" :precision="2" :step="0.01" style="width: 100%" />
                        </template>
                    </el-table-column>
                    <el-table-column label="Cost Rate" width="150">
                        <template #default="{ row }">
                            <el-input-number v-model="row.costRate" :min="0" :precision="2" :step="0.01" style="width: 100%" />
                        </template>
                    </el-table-column>
                    <el-table-column label="Retail Rate" width="150">
                        <template #default="{ row }">
                            <el-input-number v-model="row.retailRate" :min="0" :precision="2" :step="0.01" style="width: 100%" />
                        </template>
                    </el-table-column>
                    <el-table-column label="Wholesale Rate" width="150">
                        <template #default="{ row }">
                            <el-input-number v-model="row.wholesaleRate" :min="0" :precision="2" :step="0.01" style="width: 100%" />
                        </template>
                    </el-table-column>
                    <el-table-column label="MRP" width="150">
                        <template #default="{ row }">
                            <el-input-number v-model="row.mrp" :min="0" :precision="2" :step="0.01" style="width: 100%" />
                        </template>
                    </el-table-column>
                    <el-table-column label="Batch Number" width="150">
                        <template #default="{ row }">
                            <el-input v-model="row.batchNumber" />
                        </template>
                    </el-table-column>
                    <el-table-column label="Expiry Date" width="180">
                        <template #default="{ row }">
                            <el-date-picker v-model="row.expiryDate" type="date" style="width: 100%" />
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
                        v-for="(row, index) in form.openingStockDetails"
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
                                <el-input-number v-model="row.quantity" :min="0" :step="1" style="width: 100%" />
                            </label>
                            <label>
                                <span class="mobile-field-label">Purchase Rate</span>
                                <el-input-number v-model="row.purchaseRate" :min="0" :precision="2" :step="0.01" style="width: 100%" />
                            </label>
                            <label>
                                <span class="mobile-field-label">Cost Rate</span>
                                <el-input-number v-model="row.costRate" :min="0" :precision="2" :step="0.01" style="width: 100%" />
                            </label>
                            <label>
                                <span class="mobile-field-label">Retail Rate</span>
                                <el-input-number v-model="row.retailRate" :min="0" :precision="2" :step="0.01" style="width: 100%" />
                            </label>
                            <label>
                                <span class="mobile-field-label">Wholesale Rate</span>
                                <el-input-number v-model="row.wholesaleRate" :min="0" :precision="2" :step="0.01" style="width: 100%" />
                            </label>
                            <label>
                                <span class="mobile-field-label">MRP</span>
                                <el-input-number v-model="row.mrp" :min="0" :precision="2" :step="0.01" style="width: 100%" />
                            </label>
                            <label>
                                <span class="mobile-field-label">Batch Number</span>
                                <el-input v-model="row.batchNumber" />
                            </label>
                            <label>
                                <span class="mobile-field-label">Expiry Date</span>
                                <el-date-picker v-model="row.expiryDate" type="date" style="width: 100%" />
                            </label>
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
import { ref, onMounted } from "vue";
import { ElMessage, ElMessageBox, type FormInstance, type FormRules } from "element-plus";
import { Plus } from "@element-plus/icons-vue";
import { useOpeningStockStore } from "@/stores/opening-stocks";
import { useItemStore } from "@/stores/items";
import type { CreateOpeningStock, UpdateOpeningStock, CreateOpeningStockDetail } from "@/services/opening-stocks";

const openingStockStore = useOpeningStockStore();
const itemStore = useItemStore();

const searchTerm = ref("");
const dialogVisible = ref(false);
const editingOpeningStock = ref<any>(null);
const formRef = ref<FormInstance>();
const dialogLoading = ref(false);
const saveLoading = ref(false);

const form = ref<CreateOpeningStock>({
    date: new Date().toISOString().split("T")[0],
    notes: "",
    openingStockDetails: []
});

const rules: FormRules = {
    date: [{ required: true, message: "Date is required", trigger: "change" }]
};

const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleDateString();
};

let debounceTimer: ReturnType<typeof setTimeout> | null = null;
const debouncedSearch = () => {
    if (debounceTimer) clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
        loadOpeningStocks();
    }, 300);
};

const loadOpeningStocks = () => {
    openingStockStore.fetchPaged(
        openingStockStore.pagedOpeningStocks.pageNumber,
        openingStockStore.pagedOpeningStocks.pageSize,
        searchTerm.value
    );
};

const openDialog = async (openingStock?: any) => {
    editingOpeningStock.value = openingStock || null;
    if (openingStock) {
        form.value = {
            date: openingStock.date.split("T")[0],
            notes: openingStock.notes,
            openingStockDetails: openingStock.openingStockDetails
        };
    } else {
        form.value = {
            date: new Date().toISOString().split("T")[0],
            notes: "",
            openingStockDetails: []
        };
    }
    dialogVisible.value = true;
    dialogLoading.value = true;
    try {
        await itemStore.fetchAll();
    } finally {
        dialogLoading.value = false;
    }
};

const addItemRow = () => {
    form.value.openingStockDetails.push({
        itemId: 0,
        purchaseRate: 0,
        costRate: 0,
        retailRate: 0,
        wholesaleRate: 0,
        mrp: 0,
        quantity: 0,
        batchNumber: "",
        expiryDate: ""
    });
};

const removeItemRow = (index: number) => {
    form.value.openingStockDetails.splice(index, 1);
};

const onItemSelected = (index: number) => {
    const detail = form.value.openingStockDetails[index];
    const item = itemStore.items.find(i => i.id === detail.itemId);
    if (item) {
        detail.purchaseRate = item.purchaseRate;
        detail.costRate = item.costRate;
        detail.retailRate = item.retailRate;
        detail.wholesaleRate = item.wholesaleRate;
        detail.mrp = item.mrp;
    }
};

const handleSubmit = async () => {
    if (!formRef.value || saveLoading.value) return;
    const valid = await formRef.value.validate().catch(() => false);
    if (!valid) return;

    const hasInvalidItem = form.value.openingStockDetails.length === 0
        || form.value.openingStockDetails.some(detail => !detail.itemId);

    if (hasInvalidItem) {
        ElMessage.warning("Add at least one valid item before saving opening stock");
        return;
    }

    saveLoading.value = true;
    try {
        let success = false;
        if (editingOpeningStock.value) {
            success = await openingStockStore.update({
                id: editingOpeningStock.value.id,
                ...form.value
            } as UpdateOpeningStock);
        } else {
            success = await openingStockStore.create(form.value);
        }
        if (success) {
            dialogVisible.value = false;
            loadOpeningStocks();
        }
    } finally {
        saveLoading.value = false;
    }
};

const handleDelete = async (id: number) => {
    try {
        await ElMessageBox.confirm("Are you sure you want to delete this opening stock?", "Confirm", {
            type: "warning"
        });
        const success = await openingStockStore.remove(id);
        if (success) {
            loadOpeningStocks();
        }
    } catch {
        // User cancelled
    }
};

onMounted(() => {
    loadOpeningStocks();
});
</script>
