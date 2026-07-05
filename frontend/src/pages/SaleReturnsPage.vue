<template>
  <div class="erp-page">
    <div class="erp-toolbar">
      <el-input
        v-model="searchTerm"
        placeholder="Search sale returns..."
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
        Add Sale Return
      </el-button>
    </div>

    <el-card>
      <el-table
        :data="saleReturnStore.pagedSaleReturns.items"
        v-loading="saleReturnStore.loading"
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
        v-model:current-page="saleReturnStore.pagedSaleReturns.pageNumber"
        v-model:page-size="saleReturnStore.pagedSaleReturns.pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="saleReturnStore.pagedSaleReturns.totalCount"
        layout="total, sizes, prev, pager, next, jumper"
        class="mt-4"
        @size-change="loadSaleReturns"
        @current-change="loadSaleReturns"
      />
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="editingReturn ? 'Edit Sale Return' : 'Add Sale Return'"
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
            <el-form-item label="Sale" prop="saleId">
              <el-select
                v-model="form.saleId"
                placeholder="Select sale invoice"
                filterable
                style="width: 100%"
                @change="onSaleSelected"
              >
                <el-option
                  v-for="sale in saleOptions"
                  :key="sale.id"
                  :label="`${sale.invoiceNumber} - ${formatDate(sale.saleDate)} - ${formatCurrency(sale.netAmount)}`"
                  :value="sale.id"
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
          <el-button :disabled="!selectedSale" @click="resetLinesFromSale">
            Reload Sale Lines
          </el-button>
          <div class="erp-total-pill">
            Total:
            <span class="text-green-600">{{ formatCurrency(totalAmount) }}</span>
          </div>
        </div>

        <el-alert
          v-if="!selectedSale"
          type="info"
          show-icon
          :closable="false"
          title="Select a sale invoice to load returnable item lines."
          class="mb-4"
        />

        <el-table
          v-else
          :data="form.saleReturnDetails"
          border
          style="width: 100%"
          empty-text="No sale lines found"
        >
          <el-table-column label="Item" min-width="220">
            <template #default="{ row }">
              <div class="font-semibold">{{ getSaleLineName(row.saleDetailId) }}</div>
              <div class="text-xs text-gray-500">Line #{{ row.saleDetailId }}</div>
            </template>
          </el-table-column>
          <el-table-column label="Sold" width="110">
            <template #default="{ row }">
              {{ getSoldQuantity(row.saleDetailId) }}
            </template>
          </el-table-column>
          <el-table-column label="Current Stock" width="120">
            <template #default="{ row }">
              {{ getItemStock(row.itemId) }}
            </template>
          </el-table-column>
          <el-table-column label="Return Qty" width="150">
            <template #default="{ row }">
              <el-input-number
                v-model="row.quantity"
                :min="0"
                :max="row.sourceQuantity"
                :step="1"
                style="width: 100%"
                @change="calculateTotals"
              />
            </template>
          </el-table-column>
          <el-table-column label="Rate" width="120">
            <template #default="{ row }">
              {{ formatCurrency(row.rate) }}
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
import { useSaleReturnStore } from "@/stores/companiesAndReturns";
import {
  saleReturnService,
  type CreateSaleReturnDetail,
  type SaleReturn,
  type UpdateSaleReturn,
} from "@/services/companiesAndReturns";
import { saleService, type Sale, type SaleDetail } from "@/services/sales";
import { useItemStore } from "@/stores/items";
import { documentNumberService } from "@/services/document-numbers";

type SaleReturnLine = CreateSaleReturnDetail & {
  sourceQuantity: number;
};

const saleReturnStore = useSaleReturnStore();
const itemStore = useItemStore();

const searchTerm = ref("");
const dialogVisible = ref(false);
const editingReturn = ref<SaleReturn | null>(null);
const formRef = ref<FormInstance>();
const dialogLoading = ref(false);
const saveLoading = ref(false);
const saleOptions = ref<Sale[]>([]);
const selectedSale = ref<Sale | null>(null);

const form = ref<{
  saleId: number;
  returnDate: string;
  returnNumber: string;
  notes?: string;
  saleReturnDetails: SaleReturnLine[];
}>({
  saleId: 0,
  returnDate: new Date().toISOString().split("T")[0],
  returnNumber: "",
  notes: "",
  saleReturnDetails: [],
});

const rules: FormRules = {
  saleId: [{ required: true, message: "Sale is required", trigger: "change" }],
  returnNumber: [{ required: true, message: "Return number is required", trigger: "blur" }],
  returnDate: [{ required: true, message: "Return date is required", trigger: "change" }],
};

const totalAmount = computed(() => {
  return form.value.saleReturnDetails.reduce(
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

const calculateLineTotal = (detail: SaleReturnLine) => {
  if (!detail.quantity) return 0;
  const lineTotal = detail.quantity * detail.rate;
  const afterDiscount = lineTotal - detail.discount;
  const taxAmount = afterDiscount * (detail.taxPercentage / 100);
  return Math.max(afterDiscount + taxAmount, 0);
};

const calculateTotals = () => {
  // Recomputes through Vue reactivity.
};

const getSaleDetail = (saleDetailId: number) => {
  return selectedSale.value?.saleDetails.find((detail) => detail.id === saleDetailId);
};

const getSaleLineName = (saleDetailId: number) => {
  const detail = getSaleDetail(saleDetailId);
  return detail?.item?.itemName || itemStore.items.find((item) => item.id === detail?.itemId)?.itemName || `Item ${detail?.itemId ?? ""}`;
};

const getSoldQuantity = (saleDetailId: number) => {
  return getSaleDetail(saleDetailId)?.quantity ?? 0;
};

const getItemStock = (itemId: number) => {
  return itemStore.items.find((item) => item.id === itemId)?.currentStock ?? 0;
};

let debounceTimer: ReturnType<typeof setTimeout> | null = null;
const debouncedSearch = () => {
  if (debounceTimer) clearTimeout(debounceTimer);
  debounceTimer = setTimeout(loadSaleReturns, 300);
};

const loadSaleReturns = () => {
  saleReturnStore.fetchPaged(
    saleReturnStore.pagedSaleReturns.pageNumber,
    saleReturnStore.pagedSaleReturns.pageSize,
    searchTerm.value,
  );
};

const loadSaleOptions = async () => {
  const response = await saleService.getPaged(1, 1000);
  saleOptions.value = response.success ? response.data?.items || [] : [];
};

const loadNextReturnNumber = async () => {
  try {
    const response = await documentNumberService.getNext("sale-return");
    if (response.success && response.data) {
      form.value.returnNumber = response.data;
    }
  } catch {
    ElMessage.warning("Failed to load next sale return number");
  }
};

const fetchSale = async (saleId: number) => {
  const response = await saleService.getById(saleId);
  if (!response.success || !response.data) {
    throw new Error(response.message || "Failed to load sale");
  }
  selectedSale.value = response.data;
  if (!saleOptions.value.some((sale) => sale.id === response.data!.id)) {
    saleOptions.value.unshift(response.data);
  }
};

const toReturnLine = (
  detail: SaleDetail,
  existingLine?: SaleReturn["saleReturnDetails"][number],
): SaleReturnLine => ({
  saleDetailId: detail.id,
  itemId: detail.itemId,
  quantity: existingLine?.quantity ?? 0,
  rate: existingLine?.rate ?? detail.rate,
  discount: existingLine?.discount ?? 0,
  taxPercentage: existingLine?.taxPercentage ?? detail.taxPercentage,
  reason: existingLine?.reason ?? "",
  sourceQuantity: detail.quantity,
});

const rebuildLines = (existingDetails: SaleReturn["saleReturnDetails"] = []) => {
  const existingBySaleDetail = new Map(
    existingDetails.map((detail) => [detail.saleDetailId, detail]),
  );
  form.value.saleReturnDetails = (selectedSale.value?.saleDetails || []).map((detail) =>
    toReturnLine(detail, existingBySaleDetail.get(detail.id)),
  );
};

const resetLinesFromSale = () => {
  rebuildLines(editingReturn.value?.saleReturnDetails || []);
};

const onSaleSelected = async (saleId: number) => {
  if (!saleId) return;
  try {
    dialogLoading.value = true;
    await fetchSale(saleId);
    rebuildLines(editingReturn.value?.saleReturnDetails || []);
  } catch (error: any) {
    ElMessage.error(error.message || "Failed to load sale");
  } finally {
    dialogLoading.value = false;
  }
};

const openDialog = async (saleReturn?: SaleReturn) => {
  editingReturn.value = null;
  selectedSale.value = null;
  form.value = {
    saleId: 0,
    returnDate: new Date().toISOString().split("T")[0],
    returnNumber: "",
    notes: "",
    saleReturnDetails: [],
  };
  dialogVisible.value = true;
  dialogLoading.value = true;

  try {
    await Promise.all([loadSaleOptions(), itemStore.fetchAllWithStock()]);

    if (saleReturn) {
      const response = await saleReturnService.getById(saleReturn.id);
      if (!response.success || !response.data) {
        throw new Error(response.message || "Failed to load sale return");
      }

      editingReturn.value = response.data;
      form.value.saleId = response.data.saleId;
      form.value.returnDate = response.data.returnDate.split("T")[0];
      form.value.returnNumber = response.data.returnNumber;
      form.value.notes = response.data.notes || "";
      await fetchSale(response.data.saleId);
      rebuildLines(response.data.saleReturnDetails);
    } else {
      await loadNextReturnNumber();
    }
  } catch (error: any) {
    ElMessage.error(error.message || "Failed to open sale return");
    dialogVisible.value = false;
  } finally {
    dialogLoading.value = false;
  }
};

const getPositiveLines = () => {
  return form.value.saleReturnDetails.filter((detail) => detail.quantity > 0);
};

const validateLines = () => {
  const lines = getPositiveLines();
  if (lines.length === 0) {
    ElMessage.warning("Enter a return quantity for at least one sale line");
    return false;
  }

  for (const line of lines) {
    if (line.quantity > line.sourceQuantity) {
      ElMessage.warning("Return quantity cannot exceed sold quantity");
      return false;
    }
    if (line.discount > line.quantity * line.rate) {
      ElMessage.warning("Discount cannot be greater than line total");
      return false;
    }
  }

  return true;
};

const buildPayload = () => ({
  saleId: form.value.saleId,
  returnDate: form.value.returnDate,
  returnNumber: form.value.returnNumber,
  notes: form.value.notes,
  saleReturnDetails: getPositiveLines().map((detail) => ({
    saleDetailId: detail.saleDetailId,
    itemId: detail.itemId,
    quantity: detail.quantity,
    rate: detail.rate,
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
      ? await saleReturnStore.update({ id: editingReturn.value.id, ...payload } as UpdateSaleReturn)
      : await saleReturnStore.create(payload);

    if (success) {
      dialogVisible.value = false;
      loadSaleReturns();
      await itemStore.fetchAllWithStock();
    }
  } finally {
    saveLoading.value = false;
  }
};

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm("Are you sure you want to delete this sale return?", "Confirm", {
      type: "warning",
    });
    const success = await saleReturnStore.remove(id);
    if (success) {
      loadSaleReturns();
      await itemStore.fetchAllWithStock();
    }
  } catch {
    // User cancelled
  }
};

onMounted(() => {
  loadSaleReturns();
});
</script>
