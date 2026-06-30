<template>
  <div class="p-6">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-2xl font-bold text-gray-800">Customers</h1>
      <el-button type="primary" @click="openDialog">
        <el-icon><Plus /></el-icon>
        Add Customer
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
        :data="customers"
        v-loading="loading"
        stripe
        style="width: 100%"
      >
        <el-table-column prop="customerCode" label="Code" width="120" />
        <el-table-column prop="customerName" label="Name" min-width="180" />
        <el-table-column prop="contactPerson" label="Contact" width="150" />
        <el-table-column prop="mobile" label="Mobile" width="130" />
        <el-table-column prop="email" label="Email" width="180" />
        <el-table-column prop="creditLimit" label="Credit Limit" width="140">
          <template #default="{ row }">
            {{ formatCurrency(row.creditLimit) }}
          </template>
        </el-table-column>
        <el-table-column prop="isActive" label="Status" width="100">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'danger'">
              {{ row.isActive ? 'Active' : 'Inactive' }}
            </el-tag>
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
          @size-change="loadCustomers"
          @current-change="loadCustomers"
        />
      </div>
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? 'Edit Customer' : 'Add Customer'"
      width="500px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="rules"
        label-width="120px"
      >
        <el-form-item label="Customer Code" prop="customerCode">
          <el-input v-model="formData.customerCode" placeholder="Enter customer code" />
        </el-form-item>
        <el-form-item label="Customer Name" prop="customerName">
          <el-input v-model="formData.customerName" placeholder="Enter customer name" />
        </el-form-item>
        <el-form-item label="Contact Person" prop="contactPerson">
          <el-input v-model="formData.contactPerson" placeholder="Enter contact person" />
        </el-form-item>
        <el-form-item label="Mobile" prop="mobile">
          <el-input v-model="formData.mobile" placeholder="Enter mobile number" />
        </el-form-item>
        <el-form-item label="WhatsApp" prop="whatsApp">
          <el-input v-model="formData.whatsApp" placeholder="Enter WhatsApp number" />
        </el-form-item>
        <el-form-item label="Email" prop="email">
          <el-input v-model="formData.email" type="email" placeholder="Enter email" />
        </el-form-item>
        <el-form-item label="Address" prop="address">
          <el-input v-model="formData.address" type="textarea" :rows="2" placeholder="Enter address" />
        </el-form-item>
        <el-form-item label="City" prop="city">
          <el-input v-model="formData.city" placeholder="Enter city" />
        </el-form-item>
        <el-form-item label="Credit Days" prop="creditDays">
          <el-input-number v-model="formData.creditDays" :min="0" class="w-full" />
        </el-form-item>
        <el-form-item label="Credit Limit" prop="creditLimit">
          <el-input-number v-model="formData.creditLimit" :min="0" :precision="2" :step="100" class="w-full" />
        </el-form-item>
        <el-form-item label="Opening Balance" prop="openingBalance">
          <el-input-number v-model="formData.openingBalance" :precision="2" :step="100" class="w-full" />
        </el-form-item>
        <el-form-item label="Active" prop="isActive">
          <el-switch v-model="formData.isActive" />
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
import { ref, reactive, onMounted, watch } from 'vue'
import { ElMessage, ElMessageBox, type FormInstance, type FormRules } from 'element-plus'
import { Plus, Search } from '@element-plus/icons-vue'
import { customerService, type Customer, type CreateCustomer, type UpdateCustomer } from '@/services/customers'

const customers = ref<Customer[]>([])
const loading = ref(false)
const saving = ref(false)
const currentPage = ref(1)
const pageSize = ref(10)
const totalCount = ref(0)
const searchTerm = ref('')
const dialogVisible = ref(false)
const isEdit = ref(false)
const formRef = ref<FormInstance>()

const formData = reactive<CreateCustomer & { id?: number }>({
  customerCode: '',
  customerName: '',
  contactPerson: '',
  mobile: '',
  whatsApp: '',
  email: '',
  address: '',
  city: '',
  creditDays: 0,
  creditLimit: 0,
  openingBalance: 0,
  isActive: true
})

const rules: FormRules = {
  customerCode: [{ required: true, message: 'Please enter customer code', trigger: 'blur' }],
  customerName: [{ required: true, message: 'Please enter customer name', trigger: 'blur' }]
}

let debounceTimer: ReturnType<typeof setTimeout>
const debouncedSearch = () => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    currentPage.value = 1
    loadCustomers()
  }, 300)
}

const loadCustomers = async () => {
  loading.value = true
  try {
    const response = await customerService.getPaged(currentPage.value, pageSize.value, searchTerm.value)
    if (response.success && response.data) {
      customers.value = response.data.items
      totalCount.value = response.data.totalCount
    }
  } catch (error) {
    ElMessage.error('Failed to load customers')
  } finally {
    loading.value = false
  }
}

const openDialog = (customer?: Customer) => {
  isEdit.value = !!customer
  if (customer) {
    Object.assign(formData, customer)
  } else {
    Object.assign(formData, {
      customerCode: '',
      customerName: '',
      contactPerson: '',
      mobile: '',
      whatsApp: '',
      email: '',
      address: '',
      city: '',
      creditDays: 0,
      creditLimit: 0,
      openingBalance: 0,
      isActive: true,
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
          await customerService.update(formData as UpdateCustomer)
          ElMessage.success('Customer updated successfully')
        } else {
          await customerService.create(formData)
          ElMessage.success('Customer created successfully')
        }
        dialogVisible.value = false
        await loadCustomers()
      } catch (error: any) {
        ElMessage.error(error.response?.data?.message || 'Failed to save customer')
      } finally {
        saving.value = false
      }
    }
  })
}

const handleDelete = async (customer: Customer) => {
  try {
    await ElMessageBox.confirm('Are you sure you want to delete this customer?', 'Delete Customer', {
      confirmButtonText: 'Yes',
      cancelButtonText: 'No',
      type: 'warning'
    })
    await customerService.delete(customer.id)
    ElMessage.success('Customer deleted successfully')
    await loadCustomers()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('Failed to delete customer')
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
  loadCustomers()
})
</script>
