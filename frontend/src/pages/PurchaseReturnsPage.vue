<template>
  <div class="erp-page">
    <div class="erp-toolbar">
      <el-input
        v-model="searchTerm"
        placeholder="Search purchase returns..."
        prefix-icon="Search"
        class="erp-search"
        clearable
        @input="debouncedSearch"
      />
      <el-button
        type="primary"
        :loading="dialogLoading && !editingReturn"
        @click="openDialog()"
      >
        <el-icon><Plus /></el-icon>
        Add Purchase Return
      </el-button>
    </div>

    <el-card>
      <el-table
        :data="purchaseReturnStore.pagedPurchaseReturns.items"
        v-loading="purchaseReturnStore.loading"
        stripe
      >
        <el-table-column prop="id" label="ID" width="60" />
        <el-table-column prop="returnNumber" label="Return No" width="140" />
        <el-table-column prop="returnDate" label="Date" width="110">
          <template #default="{ row }">
            {{ formatDate(row.returnDate) }}
          </template>
        </el-table-column>
        <el-table-column label="Net Amount" width="130">
          <template #default="{ row }">
            {{ formatCurrency(row.netAmount) }}
          </template>
        </el-table-column>
        <el-table-column label="Actions" width="100" fixed="right" align="center">
          <template #default="{ row }">
            <el-button link type="primary" @click="openDialog(row)">Edit</el-button>
            <!-- <el-button link type="danger" @click="handleDelete(row.id)">Delete</el-button> -->
          </template>
        </el-table-column>
      </el-table>
      <el-pagination
        v-model:current-page="purchaseReturnStore.pagedPurchaseReturns.pageNumber"
        v-model:page-size="purchaseReturnStore.pagedPurchaseReturns.pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="purchaseReturnStore.pagedPurchaseReturns.totalCount"
        layout="total, sizes, prev, pager, next, jumper"
        class="mt-4"
        @size-change="loadPurchaseReturns"
        @current-change="loadPurchaseReturns"
      />
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="editingReturn ? 'Edit Purchase Return' : 'Add Purchase Return'"
      width="1200px"
      :close-on-click-modal="!saveLoading"
    >
      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="150px"
        class="erp-dialog-form"
        v-loading="dialogLoading"
      >
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="Purchase" prop="purchaseId">
              <el-select
                v-model="form.purchaseId"
                placeholder="Select purchase invoice"
                filterable
                style="width: 100%"
                @change="onPurchaseSelected"
              >
                <el-option
                  v-for="purchase in purchaseOptions"
                  :key="purchase.id"
                  :label="`${purchase.invoiceNumber} - ${formatDate(purchase.purchaseDate)} - ${formatCurrency(purchase.netAmount)}`"
                  :value="purchase.id"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Return Date" prop="returnDate">
              <el-date-picker
                v-model="form.returnDate"
                type="date"
                value-format="YYYY-MM-DD"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="Return Number" prop="returnNumber">
          <el-input v-model="form.returnNumber" />
        </el-form-item>
        <el-form-item label="Notes">
          <el-input v-model="form.notes" type="textarea" :rows="2" />
        </el-form-item>

        <el-divider content-position="left">Items</el-divider>
        <div class="erp-details-toolbar">
          <el-button :disabled="!selectedPurchase" @click="resetLinesFromPurchase">
            Reload Purchase Lines
          </el-button>
          <div class="erp-total-pill">
            Total:
            <span class="text-green-600">{{ formatCurrency(totalAmount) }}</span>
          </div>
        </div>

        <el-alert
          v-if="!selectedPurchase"
          type="info"
          show-icon
          :closable="false"
          title="Select a purchase invoice to load returnable item lines."
          class="mb-4"
        />

        <el-table
          v-else
          :data="form.purchaseReturnDetails"
          border
          style="width: 100%"
          empty-text="No purchase lines found"
        >
          <el-table-column label="Item" min-width="220">
            <template #default="{ row }">
              <div class="font-semibold">{{ getPurchaseLineName(row.purchaseDetailId) }}</div>
              <div class="text-xs text-gray-500">Line #{{ row.purchaseDetailId }}</div>
            </template>
          </el-table-column>
          <el-table-column label="Purchased" width="110">
            <template #default="{ row }">
              {{ getPurchasedQuantity(row.purchaseDetailId) }}
            </template>
          </el-table-column>
          <el-table-column label="Stock" width="110">
            <template #default="{ row }">
              <span :class="{ 'text-red-600': getStockAvailable(row.itemId) <= 0 }">
                {{ getStockAvailable(row.itemId) }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="Return Qty" width="150">
            <template #default="{ row }">
              <el-input-number
                v-model="row.quantity"
                :min="0"
                :max="getPurchaseReturnLineMax(row)"
                :step="1"
                style="width: 100%"
                @change="calculateTotals"
              />
            </template>
          </el-table-column>
          <el-table-column label="Rate" width="120">
            <template #default="{ row }">
              {{ formatCurrency(row.purchaseRate) }}
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
                @change="calculateTotals"
              />
            </template>
          </el-table-column>
          <el-table-column label="Tax %" width="100">
            <template #default="{ row }">
              {{ row.taxPercentage }}
            </template>
          </el-table-column>
          <el-table-column label="Reason" min-width="180">
            <template #default="{ row }">
              <el-input v-model="row.reason" placeholder="Optional" />
            </template>
          </el-table-column>
          <el-table-column label="Total" width="130">
            <template #default="{ row }">
              <span class="font-semibold">{{ formatCurrency(calculateLineTotal(row)) }}</span>
            </template>
          </el-table-column>
        </el-table>
      </el-form>

      <template #footer>
        <div class="dialog-footer">
          <el-button :disabled="saveLoading" @click="dialogVisible = false">Cancel</el-button>
          <el-button
            type="primary"
            :loading="saveLoading"
            :disabled="dialogLoading"
            @click="handleSubmit"
          >
            Save
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import {
  ElMessage,
  ElMessageBox,
  type FormInstance,
  type FormRules,
} from "element-plus";
import { Plus } from "@element-plus/icons-vue";
import { usePurchaseReturnStore } from "@/stores/companiesAndReturns";
import {
  purchaseReturnService,
  type CreatePurchaseReturnDetail,
  type PurchaseReturn,
  type UpdatePurchaseReturn,
} from "@/services/companiesAndReturns";
import {
  purchaseService,
  type Purchase,
  type PurchaseDetail,
} from "@/services/purchases";
import { useItemStore } from "@/stores/items";
import { documentNumberService } from "@/services/document-numbers";

type PurchaseReturnLine = CreatePurchaseReturnDetail & {
  sourceQuantity: number;
};

const purchaseReturnStore = usePurchaseReturnStore();
const itemStore = useItemStore();

const searchTerm = ref("");
const dialogVisible = ref(false);
const editingReturn = ref<PurchaseReturn | null>(null);
const formRef = ref<FormInstance>();
const dialogLoading = ref(false);
const saveLoading = ref(false);
const purchaseOptions = ref<Purchase[]>([]);
const selectedPurchase = ref<Purchase | null>(null);
const existingQuantityByItem = ref<Map<number, number>>(new Map());

const form = ref<{
  purchaseId: number;
  returnDate: string;
  returnNumber: string;
  notes?: string;
  purchaseReturnDetails: PurchaseReturnLine[];
}>({
  purchaseId: 0,
  returnDate: new Date().toISOString().split("T")[0],
  returnNumber: "",
  notes: "",
  purchaseReturnDetails: [],
});

const rules: FormRules = {
  purchaseId: [{ required: true, message: "Purchase is required", trigger: "change" }],
  returnNumber: [{ required: true, message: "Return number is required", trigger: "blur" }],
  returnDate: [{ required: true, message: "Return date is required", trigger: "change" }],
};

const totalAmount = computed(() => {
  return form.value.purchaseReturnDetails.reduce(
    (sum, detail) => sum + calculateLineTotal(detail),
    0,
  );
});

const formatDate = (dateStr: string) => new Date(dateStr).toLocaleDateString();

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "QAR",
  }).format(amount || 0);
};

const calculateLineTotal = (detail: PurchaseReturnLine) => {
  if (!detail.quantity) return 0;
  const lineTotal = detail.quantity * detail.purchaseRate;
  const afterDiscount = lineTotal - detail.discount;
  const taxAmount = afterDiscount * (detail.taxPercentage / 100);
  return Math.max(afterDiscount + taxAmount, 0);
};

const calculateTotals = () => {
  // Recomputes through Vue reactivity.
};

const getPurchaseDetail = (purchaseDetailId: number) => {
  return selectedPurchase.value?.purchaseDetails.find((detail) => detail.id === purchaseDetailId);
};

const getPurchaseLineName = (purchaseDetailId: number) => {
  const detail = getPurchaseDetail(purchaseDetailId);
  return detail?.item?.itemName || itemStore.items.find((item) => item.id === detail?.itemId)?.itemName || `Item ${detail?.itemId ?? ""}`;
};

const getPurchasedQuantity = (purchaseDetailId: number) => {
  return getPurchaseDetail(purchaseDetailId)?.quantity ?? 0;
};

const getItemStock = (itemId: number) => {
  return itemStore.items.find((item) => item.id === itemId)?.currentStock ?? 0;
};

const getStockAvailable = (itemId: number) => {
  return getItemStock(itemId) + (existingQuantityByItem.value.get(itemId) ?? 0);
};

const getPurchaseReturnLineMax = (line: PurchaseReturnLine) => {
  return Math.min(line.sourceQuantity, getStockAvailable(line.itemId));
};

let debounceTimer: ReturnType<typeof setTimeout> | null = null;
const debouncedSearch = () => {
  if (debounceTimer) clearTimeout(debounceTimer);
  debounceTimer = setTimeout(loadPurchaseReturns, 300);
};

const loadPurchaseReturns = () => {
  purchaseReturnStore.fetchPaged(
    purchaseReturnStore.pagedPurchaseReturns.pageNumber,
    purchaseReturnStore.pagedPurchaseReturns.pageSize,
    searchTerm.value,
  );
};

const loadPurchaseOptions = async () => {
  const response = await purchaseService.getPaged(1, 1000);
  purchaseOptions.value = response.success ? response.data?.items || [] : [];
};

const loadNextReturnNumber = async () => {
  try {
    const response = await documentNumberService.getNext("purchase-return");
    if (response.success && response.data) {
      form.value.returnNumber = response.data;
    }
  } catch {
    ElMessage.warning("Failed to load next purchase return number");
  }
};

const fetchPurchase = async (purchaseId: number) => {
  const response = await purchaseService.getById(purchaseId);
  if (!response.success || !response.data) {
    throw new Error(response.message || "Failed to load purchase");
  }
  selectedPurchase.value = response.data;
  if (!purchaseOptions.value.some((purchase) => purchase.id === response.data!.id)) {
    purchaseOptions.value.unshift(response.data);
  }
};

const toReturnLine = (
  detail: PurchaseDetail,
  existingLine?: PurchaseReturn["purchaseReturnDetails"][number],
): PurchaseReturnLine => ({
  purchaseDetailId: detail.id,
  itemId: detail.itemId,
  quantity: existingLine?.quantity ?? 0,
  purchaseRate: existingLine?.purchaseRate ?? detail.purchaseRate,
  costRate: existingLine?.costRate ?? detail.costRate,
  retailRate: existingLine?.retailRate ?? detail.retailRate,
  wholesaleRate: existingLine?.wholesaleRate ?? detail.wholesaleRate,
  mrp: existingLine?.mrp ?? detail.mrp,
  discount: existingLine?.discount ?? 0,
  taxPercentage: existingLine?.taxPercentage ?? detail.taxPercentage,
  reason: existingLine?.reason ?? "",
  sourceQuantity: detail.quantity,
});

const rebuildLines = (existingDetails: PurchaseReturn["purchaseReturnDetails"] = []) => {
  const existingByPurchaseDetail = new Map(
    existingDetails.map((detail) => [detail.purchaseDetailId, detail]),
  );
  form.value.purchaseReturnDetails = (selectedPurchase.value?.purchaseDetails || []).map((detail) =>
    toReturnLine(detail, existingByPurchaseDetail.get(detail.id)),
  );
  existingQuantityByItem.value = new Map();
  existingDetails.forEach((detail) => {
    existingQuantityByItem.value.set(
      detail.itemId,
      (existingQuantityByItem.value.get(detail.itemId) ?? 0) + detail.quantity,
    );
  });
};

const resetLinesFromPurchase = () => {
  rebuildLines(editingReturn.value?.purchaseReturnDetails || []);
};

const onPurchaseSelected = async (purchaseId: number) => {
  if (!purchaseId) return;
  try {
    dialogLoading.value = true;
    await fetchPurchase(purchaseId);
    rebuildLines(editingReturn.value?.purchaseReturnDetails || []);
  } catch (error: any) {
    ElMessage.error(error.message || "Failed to load purchase");
  } finally {
    dialogLoading.value = false;
  }
};

const openDialog = async (purchaseReturn?: PurchaseReturn) => {
  editingReturn.value = null;
  selectedPurchase.value = null;
  existingQuantityByItem.value = new Map();
  form.value = {
    purchaseId: 0,
    returnDate: new Date().toISOString().split("T")[0],
    returnNumber: "",
    notes: "",
    purchaseReturnDetails: [],
  };
  dialogVisible.value = true;
  dialogLoading.value = true;

  try {
    await Promise.all([loadPurchaseOptions(), itemStore.fetchAllWithStock()]);

    if (purchaseReturn) {
      const response = await purchaseReturnService.getById(purchaseReturn.id);
      if (!response.success || !response.data) {
        throw new Error(response.message || "Failed to load purchase return");
      }

      editingReturn.value = response.data;
      form.value.purchaseId = response.data.purchaseId;
      form.value.returnDate = response.data.returnDate.split("T")[0];
      form.value.returnNumber = response.data.returnNumber;
      form.value.notes = response.data.notes || "";
      await fetchPurchase(response.data.purchaseId);
      rebuildLines(response.data.purchaseReturnDetails);
    } else {
      await loadNextReturnNumber();
    }
  } catch (error: any) {
    ElMessage.error(error.message || "Failed to open purchase return");
    dialogVisible.value = false;
  } finally {
    dialogLoading.value = false;
  }
};

const getPositiveLines = () => {
  return form.value.purchaseReturnDetails.filter((detail) => detail.quantity > 0);
};

const validateLines = () => {
  const lines = getPositiveLines();
  if (lines.length === 0) {
    ElMessage.warning("Enter a return quantity for at least one purchase line");
    return false;
  }

  const quantityByItem = new Map<number, number>();
  for (const line of lines) {
    if (line.quantity > line.sourceQuantity) {
      ElMessage.warning("Return quantity cannot exceed purchased quantity");
      return false;
    }
    if (line.discount > line.quantity * line.purchaseRate) {
      ElMessage.warning("Discount cannot be greater than line total");
      return false;
    }
    quantityByItem.set(line.itemId, (quantityByItem.get(line.itemId) ?? 0) + line.quantity);
  }

  for (const [itemId, quantity] of quantityByItem) {
    if (quantity > getStockAvailable(itemId)) {
      ElMessage.warning(`Insufficient stock for item ${itemId}`);
      return false;
    }
  }

  return true;
};

const buildPayload = () => ({
  purchaseId: form.value.purchaseId,
  returnDate: form.value.returnDate,
  returnNumber: form.value.returnNumber,
  notes: form.value.notes,
  purchaseReturnDetails: getPositiveLines().map((detail) => ({
    purchaseDetailId: detail.purchaseDetailId,
    itemId: detail.itemId,
    quantity: detail.quantity,
    purchaseRate: detail.purchaseRate,
    costRate: detail.costRate,
    retailRate: detail.retailRate,
    wholesaleRate: detail.wholesaleRate,
    mrp: detail.mrp,
    discount: detail.discount,
    taxPercentage: detail.taxPercentage,
    reason: detail.reason,
  })),
});

const handleSubmit = async () => {
  if (!formRef.value || saveLoading.value) return;
  const valid = await formRef.value.validate().catch(() => false);
  if (!valid || !validateLines()) return;

  saveLoading.value = true;
  try {
    const payload = buildPayload();
    const success = editingReturn.value
      ? await purchaseReturnStore.update({ id: editingReturn.value.id, ...payload } as UpdatePurchaseReturn)
      : await purchaseReturnStore.create(payload);

    if (success) {
      dialogVisible.value = false;
      loadPurchaseReturns();
      await itemStore.fetchAllWithStock();
    }
  } finally {
    saveLoading.value = false;
  }
};

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm("Are you sure you want to delete this purchase return?", "Confirm", {
      type: "warning",
    });
    const success = await purchaseReturnStore.remove(id);
    if (success) {
      loadPurchaseReturns();
      await itemStore.fetchAllWithStock();
    }
  } catch {
    // User cancelled
  }
};

onMounted(() => {
  loadPurchaseReturns();
});
</script>
