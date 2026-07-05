<template>
  <div class="users-page">
    <div class="page-header flex justify-between items-center mb-6">
      <h2 class="text-2xl font-bold text-gray-800">User Management</h2>
      <el-button type="primary" @click="handleCreate">
        <el-icon class="mr-2"><Plus /></el-icon> Add User
      </el-button>
    </div>

    <el-card shadow="sm">
      <el-table :data="users" style="width: 100%" v-loading="loading">
        <el-table-column prop="id" label="ID" width="60" />
        <el-table-column prop="username" label="Username" width="90" />
        <!-- <el-table-column prop="fullName" label="Full Name" width="60"/> -->
        <el-table-column prop="roleName" label="Role" width="100" />
        <el-table-column prop="isActive" label="Status" width="65">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'danger'">
              {{ row.isActive ? "Active" : "Inactive" }}
            </el-tag>
          </template>
        </el-table-column>
        <!-- <el-table-column label="Actions" width="150" align="center">
          <template #default="{ row }">
            <el-button-group>
              <el-button type="primary" link @click="handleEdit(row)">
                <el-icon><Edit /></el-icon>
              </el-button>
            </el-button-group>
          </template>
        </el-table-column> -->
        <el-table-column label="Actions" width="70" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleEdit(row)">
              Edit
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? 'Edit User' : 'Create User'"
      width="500px"
      destroy-on-close
    >
      <el-form ref="formRef" :model="form" :rules="rules" label-width="120px">
        <el-form-item label="Username" prop="username">
          <el-input v-model="form.username" :disabled="isEdit" />
        </el-form-item>
        <el-form-item v-if="!isEdit" label="Password" prop="password">
          <el-input
            v-model="form.password"
            type="password"
            show-password
            autocomplete="new-password"
          />
        </el-form-item>
        <el-form-item label="Full Name" prop="fullName">
          <el-input v-model="form.fullName" />
        </el-form-item>
        <el-form-item label="Email" prop="email">
          <el-input v-model="form.email" />
        </el-form-item>
        <el-form-item label="Mobile" prop="mobile">
          <el-input v-model="form.mobile" />
        </el-form-item>
        <el-form-item label="Role" prop="roleId">
          <el-select
            v-model="form.roleId"
            placeholder="Select Role"
            class="w-full"
          >
            <el-option
              v-for="role in roles"
              :key="role.id"
              :label="role.roleName"
              :value="role.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Status" prop="isActive">
          <el-switch
            v-model="form.isActive"
            active-text="Active"
            inactive-text="Inactive"
          />
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
import { ref, reactive, onMounted } from "vue";
import { Plus, Edit } from "@element-plus/icons-vue";
import { ElMessage, type FormInstance, type FormRules } from "element-plus";
import api from "@/services/api";

interface UserDto {
  id: number;
  username: string;
  fullName: string;
  email: string | null;
  mobile: string | null;
  roleId: number;
  roleName: string;
  isActive: boolean;
}

interface Role {
  id: number;
  roleName: string;
}

const loading = ref(false);
const submitting = ref(false);
const dialogVisible = ref(false);
const isEdit = ref(false);
const users = ref<UserDto[]>([]);
const roles = ref<Role[]>([]);
const formRef = ref<FormInstance>();

const form = reactive({
  id: 0,
  username: "",
  password: "",
  fullName: "",
  email: "",
  mobile: "",
  roleId: null as number | null,
  isActive: true,
});

const rules = reactive<FormRules>({
  username: [
    { required: true, message: "Please enter username", trigger: "blur" },
  ],
  password: [
    {
      validator: (_rule, value, callback) => {
        if (!isEdit.value && !value) {
          callback(new Error("Please enter password"));
          return;
        }
        callback();
      },
      trigger: "blur",
    },
  ],
  fullName: [
    { required: true, message: "Please enter full name", trigger: "blur" },
  ],
  roleId: [
    { required: true, message: "Please select a role", trigger: "change" },
  ],
});

const fetchUsersAndRoles = async () => {
  loading.value = true;
  try {
    const resRoles = await api.get<Role[]>("/roles");
    roles.value = resRoles.data;

    const resUsers = await api.get<UserDto[]>("/users");
    users.value = resUsers.data;
  } catch (error) {
    ElMessage.error("Failed to load data");
  } finally {
    loading.value = false;
  }
};

const handleCreate = () => {
  isEdit.value = false;
  form.id = 0;
  form.username = "";
  form.password = "";
  form.fullName = "";
  form.email = "";
  form.mobile = "";
  form.roleId = null;
  form.isActive = true;
  dialogVisible.value = true;
};

const handleEdit = (row: UserDto) => {
  isEdit.value = true;
  form.id = row.id;
  form.username = row.username;
  form.password = "";
  form.fullName = row.fullName;
  form.email = row.email || "";
  form.mobile = row.mobile || "";
  form.roleId = row.roleId;
  form.isActive = row.isActive;
  dialogVisible.value = true;
};

const submitForm = async () => {
  if (!formRef.value) return;
  await formRef.value.validate(async (valid) => {
    if (valid) {
      submitting.value = true;
      try {
        const payload = {
          id: form.id,
          username: form.username,
          password: form.password,
          fullName: form.fullName,
          email: form.email,
          mobile: form.mobile,
          roleId: form.roleId,
          isActive: form.isActive,
        };

        if (isEdit.value) {
          await api.put(`/users/${form.id}`, payload);
          ElMessage.success("User updated successfully");
        } else {
          await api.post("/users", payload);
          ElMessage.success("User created successfully");
        }
        dialogVisible.value = false;
        fetchUsersAndRoles();
      } catch (error) {
        ElMessage.error("Failed to save user");
      } finally {
        submitting.value = false;
      }
    }
  });
};

onMounted(() => {
  fetchUsersAndRoles();
});
</script>
