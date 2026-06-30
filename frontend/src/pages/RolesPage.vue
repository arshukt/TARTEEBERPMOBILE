<template>
  <div class="roles-page">
    <div class="page-header flex justify-between items-center mb-6">
      <h2 class="text-2xl font-bold text-gray-800">Roles Management</h2>
      <el-button type="primary" @click="handleCreate">
        <el-icon class="mr-2"><Plus /></el-icon> Add Role
      </el-button>
    </div>

    <el-card shadow="sm">
      <el-table :data="roles" style="width: 100%" v-loading="loading">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="roleName" label="Role Name" width="200" />
        <el-table-column prop="description" label="Description" />
        <el-table-column prop="isActive" label="Status" width="100">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'danger'">
              {{ row.isActive ? 'Active' : 'Inactive' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="Actions" width="150" align="center">
          <template #default="{ row }">
            <el-button-group>
              <el-button type="primary" link @click="handleEdit(row)">
                <el-icon><Edit /></el-icon>
              </el-button>
              <el-button type="danger" link @click="handleDelete(row)">
                <el-icon><Delete /></el-icon>
              </el-button>
            </el-button-group>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? 'Edit Role' : 'Create Role'"
      width="600px"
      destroy-on-close
    >
      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="120px"
      >
        <el-form-item label="Role Name" prop="roleName">
          <el-input v-model="form.roleName" placeholder="e.g., Cashier" />
        </el-form-item>
        <el-form-item label="Description" prop="description">
          <el-input v-model="form.description" type="textarea" rows="2" />
        </el-form-item>
        <el-form-item label="Status" prop="isActive">
          <el-switch v-model="form.isActive" active-text="Active" inactive-text="Inactive" />
        </el-form-item>
        <el-form-item label="Permissions">
          <div class="permissions-container w-full border rounded p-4 max-h-[300px] overflow-y-auto bg-gray-50">
            <el-checkbox-group v-model="form.permissions" class="flex flex-col gap-2">
              <el-checkbox v-for="perm in availablePermissions" :key="perm.key" :label="perm.key">
                {{ perm.label }}
              </el-checkbox>
            </el-checkbox-group>
          </div>
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="dialogVisible = false">Cancel</el-button>
          <el-button type="primary" @click="submitForm" :loading="submitting">
            Save
          </el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { Plus, Edit, Delete } from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox, type FormInstance, type FormRules } from 'element-plus'
import api from '@/services/api'

interface Permission {
  permissionKey: string;
}

interface Role {
  id: number;
  roleName: string;
  description: string;
  isActive: boolean;
  permissions: Permission[];
}

const loading = ref(false)
const submitting = ref(false)
const dialogVisible = ref(false)
const isEdit = ref(false)
const roles = ref<Role[]>([])
const formRef = ref<FormInstance>()

const availablePermissions = [
  { key: 'Dashboard', label: 'Dashboard' },
  { key: 'Company', label: 'Company Settings' },
  { key: 'Customers', label: 'Customers' },
  { key: 'Suppliers', label: 'Suppliers' },
  { key: 'Categories', label: 'Categories' },
  { key: 'Brands', label: 'Brands' },
  { key: 'Units', label: 'Units' },
  { key: 'Items', label: 'Items' },
  { key: 'OpeningStocks', label: 'Opening Stock' },
  { key: 'Purchases', label: 'Purchases' },
  { key: 'PurchaseReturns', label: 'Purchase Returns' },
  { key: 'Sales', label: 'Sales' },
  { key: 'SaleReturns', label: 'Sales Returns' },
  { key: 'StockAdjustments', label: 'Stock Adjustments' },
  { key: 'StockTransfers', label: 'Stock Transfers' },
  { key: 'StockLedger', label: 'Stock Ledger' },
  { key: 'Payments', label: 'Payments' },
  { key: 'Reports', label: 'Reports' },
  { key: 'Roles', label: 'Role Management' },
  { key: 'Users', label: 'User Management' },
]

const form = reactive({
  id: 0,
  roleName: '',
  description: '',
  isActive: true,
  permissions: [] as string[]
})

const rules = reactive<FormRules>({
  roleName: [
    { required: true, message: 'Please enter role name', trigger: 'blur' }
  ]
})

const fetchRoles = async () => {
  loading.value = true
  try {
    const res = await api.get<Role[]>('/roles')
    roles.value = res.data
  } catch (error) {
    ElMessage.error('Failed to load roles')
  } finally {
    loading.value = false
  }
}

const handleCreate = () => {
  isEdit.value = false
  form.id = 0
  form.roleName = ''
  form.description = ''
  form.isActive = true
  form.permissions = []
  dialogVisible.value = true
}

const handleEdit = (row: Role) => {
  isEdit.value = true
  form.id = row.id
  form.roleName = row.roleName
  form.description = row.description
  form.isActive = row.isActive
  form.permissions = row.permissions?.map(p => p.permissionKey) || []
  dialogVisible.value = true
}

const submitForm = async () => {
  if (!formRef.value) return
  await formRef.value.validate(async (valid) => {
    if (valid) {
      submitting.value = true
      try {
        const payload = {
          id: form.id,
          roleName: form.roleName,
          description: form.description,
          isActive: form.isActive,
          permissions: form.permissions.map(p => ({ permissionKey: p }))
        }
        
        if (isEdit.value) {
          await api.put(`/roles/${form.id}`, payload)
          ElMessage.success('Role updated successfully')
        } else {
          await api.post('/roles', payload)
          ElMessage.success('Role created successfully')
        }
        dialogVisible.value = false
        fetchRoles()
      } catch (error) {
        ElMessage.error('Failed to save role')
      } finally {
        submitting.value = false
      }
    }
  })
}

const handleDelete = (row: Role) => {
  ElMessageBox.confirm('Are you sure to delete this role?', 'Warning', {
    confirmButtonText: 'Yes',
    cancelButtonText: 'No',
    type: 'warning'
  }).then(async () => {
    try {
      await api.delete(`/roles/${row.id}`)
      ElMessage.success('Role deleted successfully')
      fetchRoles()
    } catch (error) {
      ElMessage.error('Failed to delete role')
    }
  }).catch(() => {})
}

onMounted(() => {
  fetchRoles()
})
</script>
