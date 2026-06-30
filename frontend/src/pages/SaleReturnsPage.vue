<template>
  <div class="p-6">
    <div class="flex justify-between items-center mb-6">
      <el-input
        v-model="searchTerm"
        placeholder="Search sale returns..."
        prefix-icon="Search"
        style="width: 300px"
        clearable
        @input="debouncedSearch"
      />
      <el-button type="primary" @click="openDialog()">
        <el-icon><Plus /></el-icon>
        Add Sale Return
      </el-button>
    </div>

    <el-card>
      <el-table :data="saleReturnStore.pagedSaleReturns.items" v-loading="saleReturnStore.loading" stripe>
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
        v-model:current-page="saleReturnStore.pagedSaleReturns.pageNumber"
        v-model:page-size="saleReturnStore.pagedSaleReturns.pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="saleReturnStore.pagedSaleReturns.totalCount"
        layout="total, sizes, prev, pager, next, jumper"
        @size-change="loadSaleReturns"
        @current-change="loadSaleReturns"
        class="mt-4"
      />
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="editingReturn ? 'Edit Sale Return' : 'Add Sale Return'"
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
import { useSaleReturnStore } from "@/stores/companiesAndReturns";
import { documentNumberService } from "@/services/document-numbers";

const saleReturnStore = useSaleReturnStore();

const searchTerm = ref("");
const dialogVisible = ref(false);
const editingReturn = ref<any>(null);
const formRef = ref<FormInstance>();

const form = ref<any>({
  saleId: 0,
  returnDate: new Date().toISOString().split("T")[0],
  returnNumber: "",
  notes: "",
  saleReturnDetails: []
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
    loadSaleReturns();
  }, 300);
};

const loadSaleReturns = () => {
  saleReturnStore.fetchPaged(
    saleReturnStore.pagedSaleReturns.pageNumber,
    saleReturnStore.pagedSaleReturns.pageSize,
    searchTerm.value
  );
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

const openDialog = async (saleReturn?: any) => {
  editingReturn.value = saleReturn || null;
  if (saleReturn) {
    form.value = {
      ...saleReturn,
      returnDate: saleReturn.returnDate.split("T")[0]
    };
  } else {
    form.value = {
      saleId: 0,
      returnDate: new Date().toISOString().split("T")[0],
      returnNumber: "",
      notes: "",
      saleReturnDetails: []
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
        success = await saleReturnStore.update(form.value);
      } else {
        success = await saleReturnStore.create(form.value);
      }
      if (success) {
        dialogVisible.value = false;
        loadSaleReturns();
      }
    }
  });
};

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm("Are you sure you want to delete this sale return?", "Confirm", {
      type: "warning"
    });
    const success = await saleReturnStore.remove(id);
    if (success) {
      loadSaleReturns();
    }
  } catch {
    // User cancelled
  }
};

onMounted(() => {
  loadSaleReturns();
});
</script>
