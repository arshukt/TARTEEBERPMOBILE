<template>
  <div class="erp-page">
    <div class="erp-toolbar">
      <el-input
        v-model="searchTerm"
        placeholder="Search stock transfers..."
        prefix-icon="Search"
        class="erp-search"
        clearable
        @input="debouncedSearch"
      />
      <el-button type="primary" :loading="dialogLoading && !editingTransfer" @click="openDialog()">
        <el-icon><Plus /></el-icon>
        Add Stock Transfer
      </el-button>
    </div>

    <el-card>
      <el-table :data="stockTransferStore.pagedStockTransfers.items" v-loading="stockTransferStore.loading" stripe>
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="transferNumber" label="Transfer Number" width="180" />
        <el-table-column prop="transferDate" label="Date" width="150">
          <template #default="{ row }">
            {{ formatDate(row.transferDate) }}
          </template>
        </el-table-column>
        <el-table-column prop="fromLocation" label="From Location" width="150" />
        <el-table-column prop="toLocation" label="To Location" width="150" />
        <el-table-column label="Notes" prop="notes" />
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
        v-model:current-page="stockTransferStore.pagedStockTransfers.pageNumber"
        v-model:page-size="stockTransferStore.pagedStockTransfers.pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="stockTransferStore.pagedStockTransfers.totalCount"
        layout="total, sizes, prev, pager, next, jumper"
        @size-change="loadStockTransfers"
        @current-change="loadStockTransfers"
        class="mt-4"
      />
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="editingTransfer ? 'Edit Stock Transfer' : 'Add Stock Transfer'"
      width="1200px"
      :close-on-click-modal="!saveLoading"
    >
      <el-form :model="form" :rules="rules" ref="formRef" label-width="150px" class="erp-dialog-form" v-loading="dialogLoading">
        <el-form-item label="Transfer Number" prop="transferNumber">
          <el-input v-model="form.transferNumber" />
        </el-form-item>
        <el-form-item label="Transfer Date" prop="transferDate">
          <el-date-picker v-model="form.transferDate" type="date" style="width: 100%" />
        </el-form-item>
        <el-form-item label="From Location">
          <el-input v-model="form.fromLocation" />
        </el-form-item>
        <el-form-item label="To Location">
          <el-input v-model="form.toLocation" />
        </el-form-item>
        <el-form-item label="Notes">
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
          <el-table :data="form.stockTransferDetails" border style="width: 100%">
            <el-table-column label="Item" min-width="280">
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
            <el-table-column label="Current Stock" width="100">
              <template #default="{ row }">
                <span :class="{ 'text-red-600': (getItemById(row.itemId)?.currentStock ?? 0) <= 0 && getItemById(row.itemId)?.currentStock !== undefined }">
                  {{ getItemById(row.itemId)?.currentStock ?? '-' }}
                </span>
              </template>
            </el-table-column>
            <el-table-column label="Quantity" width="160">
              <template #default="{ row }">
                <el-input-number v-model="row.quantity" :min="1" :precision="2" :step="1" style="width: 100%" />
              </template>
            </el-table-column>
            <el-table-column label="Actions" width="90">
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
            v-for="(row, index) in form.stockTransferDetails"
            :key="index"
            class="mobile-line-item"
          >
            <div class="mobile-line-item-header">
              <span class="mobile-line-item-title">Item {{ Number(index) + 1 }}</span>
              <el-button type="danger" size="small" link @click="removeItemRow(Number(index))">
                Delete
              </el-button>
            </div>
            <div class="mobile-line-grid">
              <label class="full">
                <span class="mobile-field-label">Item</span>
                <el-select v-model="row.itemId" placeholder="Select item" style="width: 100%" @change="onItemSelected(Number(index))">
                  <el-option
                    v-for="item in itemStore.items"
                    :key="item.id"
                    :label="item.itemName"
                    :value="item.id"
                  />
                </el-select>
              </label>
              <label class="full">
                <span class="mobile-field-label">Quantity</span>
                <el-input-number v-model="row.quantity" :min="1" :precision="2" :step="1" style="width: 100%" />
              </label>
              <label class="full">
                <span class="mobile-field-label">Current Stock</span>
                <span :class="{ 'text-red-600': (getItemById(row.itemId)?.currentStock ?? 0) <= 0 && getItemById(row.itemId)?.currentStock !== undefined }">
                  {{ getItemById(row.itemId)?.currentStock ?? '-' }}
                </span>
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
import { useStockTransferStore } from "@/stores/companiesAndReturns";
import { useItemStore } from "@/stores/items";

const stockTransferStore = useStockTransferStore();
const itemStore = useItemStore();

const searchTerm = ref("");
const dialogVisible = ref(false);
const editingTransfer = ref<any>(null);
const formRef = ref<FormInstance>();
const dialogLoading = ref(false);
const saveLoading = ref(false);

const form = ref<any>({
  transferDate: new Date().toISOString().split("T")[0],
  transferNumber: "",
  fromLocation: "",
  toLocation: "",
  notes: "",
  stockTransferDetails: []
});

const rules: FormRules = {
  transferNumber: [{ required: true, message: "Transfer number is required", trigger: "blur" }],
  transferDate: [{ required: true, message: "Transfer date is required", trigger: "change" }]
};

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString();
};

let debounceTimer: ReturnType<typeof setTimeout> | null = null;
const debouncedSearch = () => {
  if (debounceTimer) clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    loadStockTransfers();
  }, 300);
};

const loadStockTransfers = () => {
  stockTransferStore.fetchPaged(
    stockTransferStore.pagedStockTransfers.pageNumber,
    stockTransferStore.pagedStockTransfers.pageSize,
    searchTerm.value
  );
};

const openDialog = async (stockTransfer?: any) => {
  editingTransfer.value = stockTransfer || null;
  if (stockTransfer) {
    form.value = {
      ...stockTransfer,
      transferDate: stockTransfer.transferDate.split("T")[0]
    };
  } else {
    form.value = {
      transferDate: new Date().toISOString().split("T")[0],
      transferNumber: "",
      fromLocation: "",
      toLocation: "",
      notes: "",
      stockTransferDetails: []
    };
  }
  dialogVisible.value = true;
  dialogLoading.value = true;
  try {
    await itemStore.fetchAllWithStock();
  } finally {
    dialogLoading.value = false;
  }
};

const getItemById = (itemId: number) => {
  return itemStore.items.find(i => i.id === itemId);
};

const addItemRow = () => {
  form.value.stockTransferDetails.push({
    itemId: 0,
    quantity: 1
  });
};

const removeItemRow = (index: number) => {
  form.value.stockTransferDetails.splice(index, 1);
};

const onItemSelected = (index: number) => {
  const detail = form.value.stockTransferDetails[index];
  const item = itemStore.items.find((i) => i.id === detail.itemId);
  if (item) {
    const stockMsg = item.currentStock !== undefined ? `Stock: ${item.currentStock}` : "Stock: -";
    if (item.currentStock !== undefined && item.currentStock <= 0) {
      ElMessage.warning(`${item.itemName} has no stock available`);
    } else {
      ElMessage.info(`${item.itemName} - ${stockMsg}`);
    }
  }
};

const handleSubmit = async () => {
  if (!formRef.value || saveLoading.value) return;
  const valid = await formRef.value.validate().catch(() => false);
  if (!valid) return;

  const hasInvalidItem = form.value.stockTransferDetails.length === 0
    || form.value.stockTransferDetails.some((detail: any) => !detail.itemId || detail.quantity <= 0);

  if (hasInvalidItem) {
    ElMessage.warning("Add at least one valid item before saving the transfer");
    return;
  }

  const insufficientStock = form.value.stockTransferDetails.some((detail: any) => {
    const item = itemStore.items.find((i) => i.id === detail.itemId);
    return item && item.currentStock !== undefined && detail.quantity > item.currentStock;
  });

  if (insufficientStock) {
    ElMessage.warning("Cannot save transfer: one or more items have insufficient stock");
    return;
  }

  saveLoading.value = true;
  try {
    let success = false;
    if (editingTransfer.value) {
      success = await stockTransferStore.update(form.value);
    } else {
      success = await stockTransferStore.create(form.value);
    }
    if (success) {
      dialogVisible.value = false;
      loadStockTransfers();
    }
  } finally {
    saveLoading.value = false;
  }
};

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm("Are you sure you want to delete this stock transfer?", "Confirm", {
      type: "warning"
    });
    const success = await stockTransferStore.remove(id);
    if (success) {
      loadStockTransfers();
    }
  } catch {
    // User cancelled
  }
};

onMounted(() => {
  loadStockTransfers();
});
</script>
