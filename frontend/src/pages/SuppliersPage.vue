<template>
  <div class="p-6">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-2xl font-bold text-gray-800">Suppliers</h1>
      <el-button type="primary" @click="openDialog">
        <el-icon><Plus /></el-icon>
        Add Supplier
      </el-button>
    </div>

    <el-card class="mb-4">
      <el-input
        v-model="searchTerm"
        placeholder="Search by name, code, or mobile"
        :prefix-icon="Search"
        @input="debouncedSearch"
        clearable
        class="w-full md:w-1/3"
      />
    </el-card>

    <el-card>
      <el-table
        :data="suppliers"
        v-loading="loading"
        stripe
        style="width: 100%"
      >
        <el-table-column prop="supplierCode" label="Code" width="120" />
        <el-table-column prop="supplierName" label="Name" min-width="180" />
        <el-table-column prop="mobile" label="Mobile" width="130" />
        <el-table-column prop="email" label="Email" width="180" />
        <el-table-column prop="openingBalance" label="Opening Balance" width="180">
          <template #default="{ row }">
            {{ formatCurrency(row.openingBalance) }}
          </template>
        </el-table-column>
        <el-table-column label="Actions" width="180">
          <template #default="{ row }">
            <el-button link type="primary" @click="openDialog(row)">Edit</el-button>
            <el-button link type="danger" @click="handleDelete(row)">Delete</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="mt-4 flex justify-center">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="totalCount"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="loadSuppliers"
          @current-change="loadSuppliers"
        />
      </div>
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? 'Edit Supplier' : 'Add Supplier'"
      width="500px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="rules"
        label-width="140px"
      >
        <el-form-item label="Supplier Code" prop="supplierCode">
          <el-input v-model="formData.supplierCode" placeholder="Enter supplier code" />
        </el-form-item>
        <el-form-item label="Supplier Name" prop="supplierName">
          <el-input v-model="formData.supplierName" placeholder="Enter supplier name" />
        </el-form-item>
        <el-form-item label="Mobile" prop="mobile">
          <el-input v-model="formData.mobile" placeholder="Enter mobile number" />
        </el-form-item>
        <el-form-item label="Email" prop="email">
          <el-input v-model="formData.email" type="email" placeholder="Enter email" />
        </el-form-item>
        <el-form-item label="Address" prop="address">
          <el-input v-model="formData.address" type="textarea" :rows="2" placeholder="Enter address" />
        </el-form-item>
        <el-form-item label="Opening Balance" prop="openingBalance">
          <el-input-number v-model="formData.openingBalance" :precision="2" :step="100" class="w-full" />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="flex justify-end gap-2">
          <el-button @click="dialogVisible = false">Cancel</el-button>
          <el-button type="primary" @click="handleSubmit" :loading="saving">Save</el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox, type FormInstance, type FormRules } from 'element-plus'
import { Plus, Search } from '@element-plus/icons-vue'
import { supplierService, type Supplier, type CreateSupplier, type UpdateSupplier } from '@/services/suppliers'

const suppliers = ref<Supplier[]>([])
const loading = ref(false)
const saving = ref(false)
const currentPage = ref(1)
const pageSize = ref(10)
const totalCount = ref(0)
const searchTerm = ref('')
const dialogVisible = ref(false)
const isEdit = ref(false)
const formRef = ref<FormInstance>()

const formData = reactive<CreateSupplier & { id?: number }>({
  supplierCode: '',
  supplierName: '',
  mobile: '',
  email: '',
  address: '',
  openingBalance: 0,
  id: undefined
})

const rules: FormRules = {
  supplierCode: [{ required: true, message: 'Please enter supplier code', trigger: 'blur' }],
  supplierName: [{ required: true, message: 'Please enter supplier name', trigger: 'blur' }]
}

let debounceTimer: ReturnType<typeof setTimeout>
const debouncedSearch = () => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    currentPage.value = 1
    loadSuppliers()
  }, 300)
}

const loadSuppliers = async () => {
  loading.value = true
  try {
    const response = await supplierService.getPaged(currentPage.value, pageSize.value, searchTerm.value)
    if (response.success && response.data) {
      suppliers.value = response.data.items
      totalCount.value = response.data.totalCount
    }
  } catch (error) {
    ElMessage.error('Failed to load suppliers')
  } finally {
    loading.value = false
  }
}

const openDialog = (supplier?: Supplier) => {
  isEdit.value = !!supplier
  if (supplier) {
    Object.assign(formData, supplier)
  } else {
    Object.assign(formData, {
      supplierCode: '',
      supplierName: '',
      mobile: '',
      email: '',
      address: '',
      openingBalance: 0,
      id: undefined
    })
  }
  dialogVisible.value = true
}

const handleSubmit = async () => {
  if (!formRef.value) return
  await formRef.value.validate(async (valid) => {
    if (valid) {
      saving.value = true
      try {
        if (isEdit.value && formData.id) {
          await supplierService.update(formData as UpdateSupplier)
          ElMessage.success('Supplier updated successfully')
        } else {
          await supplierService.create(formData)
          ElMessage.success('Supplier created successfully')
        }
        dialogVisible.value = false
        await loadSuppliers()
      } catch (error: any) {
        ElMessage.error(error.response?.data?.message || 'Failed to save supplier')
      } finally {
        saving.value = false
      }
    }
  })
}

const handleDelete = async (supplier: Supplier) => {
  try {
    await ElMessageBox.confirm('Are you sure you want to delete this supplier?', 'Delete Supplier', {
      confirmButtonText: 'Yes',
      cancelButtonText: 'No',
      type: 'warning'
    })
    await supplierService.delete(supplier.id)
    ElMessage.success('Supplier deleted successfully')
    await loadSuppliers()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('Failed to delete supplier')
    }
  }
}

const formatCurrency = (value: number): string => {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    minimumFractionDigits: 2
  }).format(value)
}

onMounted(() => {
  loadSuppliers()
})
</script>
