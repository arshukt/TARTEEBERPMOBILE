<template>
  <div class="p-6 max-w-2xl mx-auto">
    <el-card>
      <template #header>
        <div class="flex items-center justify-between">
          <h2 class="text-xl font-bold">User Profile</h2>
        </div>
      </template>

      <el-tabs v-model="activeTab">
        <el-tab-pane label="Profile" name="profile">
          <el-descriptions :column="1" border>
            <el-descriptions-item label="Username">{{ authStore.user?.username }}</el-descriptions-item>
            <el-descriptions-item label="Full Name">{{ authStore.user?.fullName }}</el-descriptions-item>
            <el-descriptions-item label="Mobile">{{ authStore.user?.mobile || '-' }}</el-descriptions-item>
            <el-descriptions-item label="Email">{{ authStore.user?.email || '-' }}</el-descriptions-item>
            <el-descriptions-item label="Role">{{ authStore.user?.roleName }}</el-descriptions-item>
          </el-descriptions>
        </el-tab-pane>

        <el-tab-pane label="Change Password" name="password">
          <el-form :model="passwordForm" :rules="passwordRules" ref="passwordFormRef" label-width="150px">
            <el-form-item label="Current Password" prop="currentPassword">
              <el-input
                v-model="passwordForm.currentPassword"
                type="password"
                show-password
                placeholder="Enter current password"
              />
            </el-form-item>
            <el-form-item label="New Password" prop="newPassword">
              <el-input
                v-model="passwordForm.newPassword"
                type="password"
                show-password
                placeholder="Enter new password"
              />
            </el-form-item>
            <el-form-item label="Confirm Password" prop="confirmPassword">
              <el-input
                v-model="passwordForm.confirmPassword"
                type="password"
                show-password
                placeholder="Confirm new password"
              />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" @click="handleChangePassword" :loading="loading">
                Change Password
              </el-button>
            </el-form-item>
          </el-form>
        </el-tab-pane>
      </el-tabs>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useAuthStore } from "@/stores/auth";
import { ElMessage, ElMessageBox, type FormInstance, type FormRules } from "element-plus";
import { authService } from "@/services/auth";

const authStore = useAuthStore();
const activeTab = ref("profile");
const loading = ref(false);
const passwordFormRef = ref<FormInstance>();

const passwordForm = ref({
  currentPassword: "",
  newPassword: "",
  confirmPassword: ""
});

const validateConfirmPassword = (rule: any, value: any, callback: any) => {
  if (value !== passwordForm.value.newPassword) {
    callback(new Error("Passwords do not match"));
  } else {
    callback();
  }
};

const passwordRules: FormRules = {
  currentPassword: [
    { required: true, message: "Current password is required", trigger: "blur" }
  ],
  newPassword: [
    { required: true, message: "New password is required", trigger: "blur" },
    { min: 6, message: "Password must be at least 6 characters", trigger: "blur" }
  ],
  confirmPassword: [
    { required: true, message: "Confirm password is required", trigger: "blur" },
    { validator: validateConfirmPassword, trigger: "blur" }
  ]
};

const handleChangePassword = async () => {
  if (!passwordFormRef.value) return;
  
  await passwordFormRef.value.validate(async (valid) => {
    if (valid) {
      try {
        loading.value = true;
        const response = await authService.changePassword(passwordForm.value);
        if (response.success) {
          ElMessage.success("Password changed successfully!");
          passwordForm.value = {
            currentPassword: "",
            newPassword: "",
            confirmPassword: ""
          };
        }
      } catch (error: any) {
        ElMessage.error(error?.response?.data?.message || "Failed to change password");
      } finally {
        loading.value = false;
      }
    }
  });
};
</script>
