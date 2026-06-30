<template>
  <div class="p-6">
    <div class="flex justify-between items-center mb-6">
      <el-input
        v-model="searchTerm"
        placeholder="Search purchase returns..."
        prefix-icon="Search"
        style="width: 300px"
        clearable
        @input="debouncedSearch"
      />
      <el-button type="primary" @click="openDialog()">
        <el-icon><Plus /></el-icon>
        Add Purchase Return
      </el-button>
    </div>

    <el-card>
      <el-table :data="purchaseReturnStore.pagedPurchaseReturns.items" v-loading="purchaseReturnStore.loading" stripe>
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="returnNumber" label="Return Number" width="180" />
        <el-table-column prop="returnDate" label="Date" width="150">
          <template #default="{ row }">
            {{ formatDate(row.returnDate) }}
          </template>
        </el-table-column>
        <el-table-column label="Net Amount" width="150">
          <template #default="{ row }">
            {{ formatCurrency(row.netAmount) }}
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
        v-model:current-page="purchaseReturnStore.pagedPurchaseReturns.pageNumber"
        v-model:page-size="purchaseReturnStore.pagedPurchaseReturns.pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="purchaseReturnStore.pagedPurchaseReturns.totalCount"
        layout="total, sizes, prev, pager, next, jumper"
        @size-change="loadPurchaseReturns"
        @current-change="loadPurchaseReturns"
        class="mt-4"
      />
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="editingReturn ? 'Edit Purchase Return' : 'Add Purchase Return'"
      width="1200px"
    >
      <el-form :model="form" :rules="rules" ref="formRef" label-width="150px">
        <el-form-item label="Return Number" prop="returnNumber">
          <el-input v-model="form.returnNumber" />
        </el-form-item>
        <el-form-item label="Return Date" prop="returnDate">
          <el-date-picker v-model="form.returnDate" type="date" style="width: 100%" />
        </el-form-item>
        <el-form-item label="Notes">
          <el-input v-model="form.notes" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="dialogVisible = false">Cancel</el-button>
        <el-button type="primary" @click="handleSubmit">Save</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { ElMessage, ElMessageBox, type FormInstance, type FormRules } from "element-plus";
import { Plus } from "@element-plus/icons-vue";
import { usePurchaseReturnStore } from "@/stores/companiesAndReturns";
import { documentNumberService } from "@/services/document-numbers";

const purchaseReturnStore = usePurchaseReturnStore();

const searchTerm = ref("");
const dialogVisible = ref(false);
const editingReturn = ref<any>(null);
const formRef = ref<FormInstance>();

const form = ref<any>({
  purchaseId: 0,
  returnDate: new Date().toISOString().split("T")[0],
  returnNumber: "",
  notes: "",
  purchaseReturnDetails: []
});

const rules: FormRules = {
  returnNumber: [{ required: true, message: "Return number is required", trigger: "blur" }],
  returnDate: [{ required: true, message: "Return date is required", trigger: "change" }]
};

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString();
};

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD"
  }).format(amount);
};

let debounceTimer: ReturnType<typeof setTimeout> | null = null;
const debouncedSearch = () => {
  if (debounceTimer) clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    loadPurchaseReturns();
  }, 300);
};

const loadPurchaseReturns = () => {
  purchaseReturnStore.fetchPaged(
    purchaseReturnStore.pagedPurchaseReturns.pageNumber,
    purchaseReturnStore.pagedPurchaseReturns.pageSize,
    searchTerm.value
  );
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

const openDialog = async (purchaseReturn?: any) => {
  editingReturn.value = purchaseReturn || null;
  if (purchaseReturn) {
    form.value = {
      ...purchaseReturn,
      returnDate: purchaseReturn.returnDate.split("T")[0]
    };
  } else {
    form.value = {
      purchaseId: 0,
      returnDate: new Date().toISOString().split("T")[0],
      returnNumber: "",
      notes: "",
      purchaseReturnDetails: []
    };
    await loadNextReturnNumber();
  }
  dialogVisible.value = true;
};

const handleSubmit = async () => {
  if (!formRef.value) return;
  await formRef.value.validate(async (valid) => {
    if (valid) {
      let success = false;
      if (editingReturn.value) {
        success = await purchaseReturnStore.update(form.value);
      } else {
        success = await purchaseReturnStore.create(form.value);
      }
      if (success) {
        dialogVisible.value = false;
        loadPurchaseReturns();
      }
    }
  });
};

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm("Are you sure you want to delete this purchase return?", "Confirm", {
      type: "warning"
    });
    const success = await purchaseReturnStore.remove(id);
    if (success) {
      loadPurchaseReturns();
    }
  } catch {
    // User cancelled
  }
};

onMounted(() => {
  loadPurchaseReturns();
});
</script>
