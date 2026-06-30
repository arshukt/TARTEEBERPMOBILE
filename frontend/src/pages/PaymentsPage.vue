<template>
  <div class="p-6">
    <div class="flex justify-between items-center mb-6">
      <el-input
        v-model="searchTerm"
        placeholder="Search payments..."
        prefix-icon="Search"
        style="width: 300px"
        clearable
        @input="debouncedSearch"
      />
      <el-button type="primary" @click="openDialog()">
        <el-icon><Plus /></el-icon>
        Add Payment
      </el-button>
    </div>

    <el-card>
      <el-table :data="paymentStore.pagedPayments.items" v-loading="paymentStore.loading" stripe>
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="paymentNumber" label="Payment Number" width="180" />
        <el-table-column prop="paymentDate" label="Date" width="150">
          <template #default="{ row }">
            {{ formatDate(row.paymentDate) }}
          </template>
        </el-table-column>
        <el-table-column label="Amount" width="150">
          <template #default="{ row }">
            {{ formatCurrency(row.amount) }}
          </template>
        </el-table-column>
        <el-table-column label="Payment Method" prop="paymentMethod" width="150" />
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
        v-model:current-page="paymentStore.pagedPayments.pageNumber"
        v-model:page-size="paymentStore.pagedPayments.pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="paymentStore.pagedPayments.totalCount"
        layout="total, sizes, prev, pager, next, jumper"
        @size-change="loadPayments"
        @current-change="loadPayments"
        class="mt-4"
      />
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="editingPayment ? 'Edit Payment' : 'Add Payment'"
      width="800px"
    >
      <el-form :model="form" :rules="rules" ref="formRef" label-width="150px">
        <el-form-item label="Payment Number" prop="paymentNumber">
          <el-input v-model="form.paymentNumber" />
        </el-form-item>
        <el-form-item label="Payment Date" prop="paymentDate">
          <el-date-picker v-model="form.paymentDate" type="date" style="width: 100%" />
        </el-form-item>
        <el-form-item label="Payment Type" prop="paymentType">
          <el-select v-model="form.paymentType" style="width: 100%" placeholder="Select type">
            <el-option label="Receive" :value="1" />
            <el-option label="Pay" :value="2" />
          </el-select>
        </el-form-item>
        <el-form-item label="Party Type" prop="partyType">
          <el-select v-model="form.partyType" style="width: 100%" placeholder="Select party type">
            <el-option label="Customer" :value="1" />
            <el-option label="Supplier" :value="2" />
          </el-select>
        </el-form-item>
        <el-form-item label="Amount" prop="amount">
          <el-input-number v-model="form.amount" :min="0" :precision="2" style="width: 100%" />
        </el-form-item>
        <el-form-item label="Payment Method">
          <el-input v-model="form.paymentMethod" />
        </el-form-item>
        <el-form-item label="Reference Number">
          <el-input v-model="form.referenceNumber" />
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
import { usePaymentStore } from "@/stores/companiesAndReturns";
import { documentNumberService } from "@/services/document-numbers";

const paymentStore = usePaymentStore();

const searchTerm = ref("");
const dialogVisible = ref(false);
const editingPayment = ref<any>(null);
const formRef = ref<FormInstance>();

const form = ref<any>({
  paymentDate: new Date().toISOString().split("T")[0],
  paymentNumber: "",
  paymentType: 1,
  partyType: 1,
  partyId: 0,
  amount: 0,
  paymentMethod: "",
  referenceNumber: "",
  notes: ""
});

const rules: FormRules = {
  paymentNumber: [{ required: true, message: "Payment number is required", trigger: "blur" }],
  paymentDate: [{ required: true, message: "Payment date is required", trigger: "change" }],
  paymentType: [{ required: true, message: "Payment type is required", trigger: "change" }],
  partyType: [{ required: true, message: "Party type is required", trigger: "change" }],
  amount: [{ required: true, message: "Amount is required", trigger: "blur" }]
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
    loadPayments();
  }, 300);
};

const loadPayments = () => {
  paymentStore.fetchPaged(
    paymentStore.pagedPayments.pageNumber,
    paymentStore.pagedPayments.pageSize,
    searchTerm.value
  );
};

const loadNextPaymentNumber = async () => {
  try {
    const response = await documentNumberService.getNext("payment");
    if (response.success && response.data) {
      form.value.paymentNumber = response.data;
    }
  } catch {
    ElMessage.warning("Failed to load next payment number");
  }
};

const openDialog = async (payment?: any) => {
  editingPayment.value = payment || null;
  if (payment) {
    form.value = {
      ...payment,
      paymentDate: payment.paymentDate.split("T")[0]
    };
  } else {
    form.value = {
      paymentDate: new Date().toISOString().split("T")[0],
      paymentNumber: "",
      paymentType: 1,
      partyType: 1,
      partyId: 0,
      amount: 0,
      paymentMethod: "",
      referenceNumber: "",
      notes: ""
    };
    await loadNextPaymentNumber();
  }
  dialogVisible.value = true;
};

const handleSubmit = async () => {
  if (!formRef.value) return;
  await formRef.value.validate(async (valid) => {
    if (valid) {
      let success = false;
      if (editingPayment.value) {
        success = await paymentStore.update(form.value);
      } else {
        success = await paymentStore.create(form.value);
      }
      if (success) {
        dialogVisible.value = false;
        loadPayments();
      }
    }
  });
};

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm("Are you sure you want to delete this payment?", "Confirm", {
      type: "warning"
    });
    const success = await paymentStore.remove(id);
    if (success) {
      loadPayments();
    }
  } catch {
    // User cancelled
  }
};

onMounted(() => {
  loadPayments();
});
</script>
