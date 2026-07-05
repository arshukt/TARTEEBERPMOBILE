<template>
  <div class="erp-page">
    <div class="erp-toolbar">
      <el-input
        v-model="searchTerm"
        placeholder="Search stock adjustments..."
        prefix-icon="Search"
        class="erp-search"
        clearable
        @input="debouncedSearch"
      />
      <el-button type="primary" :loading="dialogLoading && !editingAdjustment" @click="openDialog()">
        <el-icon><Plus /></el-icon>
        Add Stock Adjustment
      </el-button>
    </div>

    <el-card>
      <el-table :data="stockAdjustmentStore.pagedStockAdjustments.items" v-loading="stockAdjustmentStore.loading" stripe>
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="adjustmentNumber" label="Adjustment Number" width="180" />
        <el-table-column prop="adjustmentDate" label="Date" width="150">
          <template #default="{ row }">
            {{ formatDate(row.adjustmentDate) }}
          </template>
        </el-table-column>
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
        v-model:current-page="stockAdjustmentStore.pagedStockAdjustments.pageNumber"
        v-model:page-size="stockAdjustmentStore.pagedStockAdjustments.pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="stockAdjustmentStore.pagedStockAdjustments.totalCount"
        layout="total, sizes, prev, pager, next, jumper"
        @size-change="loadStockAdjustments"
        @current-change="loadStockAdjustments"
        class="mt-4"
      />
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="editingAdjustment ? 'Edit Stock Adjustment' : 'Add Stock Adjustment'"
      width="1200px"
      :close-on-click-modal="!saveLoading"
    >
      <el-form :model="form" :rules="rules" ref="formRef" label-width="150px" class="erp-dialog-form" v-loading="dialogLoading">
        <el-form-item label="Adjustment Number" prop="adjustmentNumber">
          <el-input v-model="form.adjustmentNumber" />
        </el-form-item>
        <el-form-item label="Adjustment Date" prop="adjustmentDate">
          <el-date-picker v-model="form.adjustmentDate" type="date" style="width: 100%" />
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
          <el-table :data="form.stockAdjustmentDetails" border style="width: 100%">
            <el-table-column label="Item" min-width="260">
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
            <el-table-column label="Quantity In" width="150">
              <template #default="{ row }">
                <el-input-number v-model="row.quantityIn" :min="0" :precision="2" :step="1" style="width: 100%" />
              </template>
            </el-table-column>
            <el-table-column label="Quantity Out" width="150">
              <template #default="{ row }">
                <el-input-number v-model="row.quantityOut" :min="0" :precision="2" :step="1" style="width: 100%" />
              </template>
            </el-table-column>
            <el-table-column label="Reason" min-width="220">
              <template #default="{ row }">
                <el-input v-model="row.reason" />
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
            v-for="(row, index) in form.stockAdjustmentDetails"
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
              <label>
                <span class="mobile-field-label">Quantity In</span>
                <el-input-number v-model="row.quantityIn" :min="0" :precision="2" :step="1" style="width: 100%" />
              </label>
              <label>
                <span class="mobile-field-label">Current Stock</span>
                <span :class="{ 'text-red-600': (getItemById(row.itemId)?.currentStock ?? 0) <= 0 && getItemById(row.itemId)?.currentStock !== undefined }">
                  {{ getItemById(row.itemId)?.currentStock ?? '-' }}
                </span>
              </label>
              <label>
                <span class="mobile-field-label">Quantity Out</span>
                <el-input-number v-model="row.quantityOut" :min="0" :precision="2" :step="1" style="width: 100%" />
              </label>
              <label class="full">
                <span class="mobile-field-label">Reason</span>
                <el-input v-model="row.reason" />
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
import { useStockAdjustmentStore } from "@/stores/companiesAndReturns";
import { useItemStore } from "@/stores/items";
import { documentNumberService } from "@/services/document-numbers";

const stockAdjustmentStore = useStockAdjustmentStore();
const itemStore = useItemStore();

const searchTerm = ref("");
const dialogVisible = ref(false);
const editingAdjustment = ref<any>(null);
const formRef = ref<FormInstance>();
const dialogLoading = ref(false);
const saveLoading = ref(false);

const form = ref<any>({
  adjustmentDate: new Date().toISOString().split("T")[0],
  adjustmentNumber: "",
  notes: "",
  stockAdjustmentDetails: []
});

const rules: FormRules = {
  adjustmentNumber: [{ required: true, message: "Adjustment number is required", trigger: "blur" }],
  adjustmentDate: [{ required: true, message: "Adjustment date is required", trigger: "change" }]
};

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString();
};

let debounceTimer: ReturnType<typeof setTimeout> | null = null;
const debouncedSearch = () => {
  if (debounceTimer) clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    loadStockAdjustments();
  }, 300);
};

const loadStockAdjustments = () => {
  stockAdjustmentStore.fetchPaged(
    stockAdjustmentStore.pagedStockAdjustments.pageNumber,
    stockAdjustmentStore.pagedStockAdjustments.pageSize,
    searchTerm.value
  );
};

const loadNextAdjustmentNumber = async () => {
  try {
    const response = await documentNumberService.getNext("stock-adjustment");
    if (response.success && response.data) {
      form.value.adjustmentNumber = response.data;
    }
  } catch {
    ElMessage.warning("Failed to load next stock adjustment number");
  }
};

const openDialog = async (stockAdjustment?: any) => {
  editingAdjustment.value = stockAdjustment || null;
  if (stockAdjustment) {
    form.value = {
      ...stockAdjustment,
      adjustmentDate: stockAdjustment.adjustmentDate.split("T")[0]
    };
  } else {
    form.value = {
      adjustmentDate: new Date().toISOString().split("T")[0],
      adjustmentNumber: "",
      notes: "",
      stockAdjustmentDetails: []
    };
  }
  dialogVisible.value = true;
  dialogLoading.value = true;
  try {
    const tasks = [itemStore.fetchAllWithStock()];
    if (!stockAdjustment) {
      tasks.push(loadNextAdjustmentNumber());
    }
    await Promise.all(tasks);
  } finally {
    dialogLoading.value = false;
  }
};

const getItemById = (itemId: number) => {
  return itemStore.items.find(i => i.id === itemId);
};

const addItemRow = () => {
  form.value.stockAdjustmentDetails.push({
    itemId: 0,
    quantityIn: 0,
    quantityOut: 0,
    reason: ""
  });
};

const removeItemRow = (index: number) => {
  form.value.stockAdjustmentDetails.splice(index, 1);
};

const onItemSelected = (index: number) => {
  const detail = form.value.stockAdjustmentDetails[index];
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

  const hasInvalidItem = form.value.stockAdjustmentDetails.length === 0
    || form.value.stockAdjustmentDetails.some((detail: any) =>
      !detail.itemId || (detail.quantityIn <= 0 && detail.quantityOut <= 0)
    );

  if (hasInvalidItem) {
    ElMessage.warning("Add at least one item with quantity in or quantity out");
    return;
  }

  const insufficientStock = form.value.stockAdjustmentDetails.some((detail: any) => {
    const item = itemStore.items.find((i) => i.id === detail.itemId);
    return item && item.currentStock !== undefined && detail.quantityOut > item.currentStock;
  });

  if (insufficientStock) {
    ElMessage.warning("Cannot save adjustment: quantity out exceeds current stock for one or more items");
    return;
  }

  saveLoading.value = true;
  try {
    let success = false;
    if (editingAdjustment.value) {
      success = await stockAdjustmentStore.update(form.value);
    } else {
      success = await stockAdjustmentStore.create(form.value);
    }
    if (success) {
      dialogVisible.value = false;
      loadStockAdjustments();
    }
  } finally {
    saveLoading.value = false;
  }
};

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm("Are you sure you want to delete this stock adjustment?", "Confirm", {
      type: "warning"
    });
    const success = await stockAdjustmentStore.remove(id);
    if (success) {
      loadStockAdjustments();
    }
  } catch {
    // User cancelled
  }
};

onMounted(() => {
  loadStockAdjustments();
});
</script>
