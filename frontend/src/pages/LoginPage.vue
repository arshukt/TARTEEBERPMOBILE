<template>
  <div class="login-page relative">
    <div class="absolute top-4 right-4 z-10">
      <!-- <el-button @click="toggleTheme" circle>
        <el-icon v-if="isDark"><Moon /></el-icon>
        <el-icon v-else><Sunny /></el-icon>
      </el-button> -->
    </div>
    <div class="delivery-background">
      <svg class="delivery-map" viewBox="0 0 1200 800">
        <!-- Route -->
        <path
          id="deliveryRoute"
          d="M120,620
         C220,500 350,480 430,390
         S640,180 780,250
         S980,470 1080,170"
        />

        <!-- Route Glow -->
        <path
          class="route-glow"
          d="M120,620
         C220,500 350,480 430,390
         S640,180 780,250
         S980,470 1080,170"
        />

        <!-- Pins -->
        <circle cx="120" cy="620" r="10" class="location-pin" />
        <circle cx="430" cy="390" r="10" class="location-pin" />
        <circle cx="780" cy="250" r="10" class="location-pin" />
        <circle cx="1080" cy="170" r="10" class="location-pin" />

        <!-- Moving Van -->
        <g class="delivery-van">
          <rect x="-14" y="-8" width="28" height="14" rx="3" />

          <circle cx="-8" cy="8" r="3" />
          <circle cx="8" cy="8" r="3" />

          <animateMotion dur="16s" repeatCount="indefinite" rotate="auto">
            <mpath href="#deliveryRoute" />
          </animateMotion>
        </g>
      </svg>
    </div>
    <el-card class="login-card">
      <template #header>
        <div class="login-brand">
          <img src="/logo.png" class="login-logo" />
          <h1 class="login-title">Tarteeb ERP</h1>
          <p>Van Sales Management</p>
        </div>
      </template>

      <el-form
        ref="loginFormRef"
        :model="loginForm"
        :rules="loginRules"
        label-position="top"
        @submit.prevent="handleLogin"
      >
        <el-form-item label="Username" prop="username">
          <el-input
            v-model="loginForm.username"
            placeholder="Enter username"
            :prefix-icon="User"
            size="large"
          />
        </el-form-item>

        <el-form-item label="Password" prop="password">
          <el-input
            v-model="loginForm.password"
            type="password"
            placeholder="Enter password"
            :prefix-icon="Lock"
            size="large"
            show-password
            @keyup.enter="handleLogin"
          />
        </el-form-item>

        <el-form-item>
          <el-button
            type="primary"
            size="large"
            class="w-full"
            :loading="loading"
            @click="handleLogin"
          >
            Login
          </el-button>
        </el-form-item>
        <div class="developer-credit">
          Powered by
          <a
            href="https://supplycosoft.com/"
            target="_blank"
            rel="noopener noreferrer"
          >
            Supplyco Soft
          </a>
        </div>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from "vue";
import { useRouter } from "vue-router";
import { ElMessage, type FormInstance, type FormRules } from "element-plus";
import { Moon, Sunny } from "@element-plus/icons-vue";
import { useAuthStore } from "@/stores/auth";
import { useTheme } from "@/composables/useTheme";
import type { LoginRequest } from "@/types";
import { User, Lock } from "@element-plus/icons-vue";

const router = useRouter();
const authStore = useAuthStore();
const { isDark, toggleTheme } = useTheme();

const loginFormRef = ref<FormInstance>();
const loading = ref(false);

const loginForm = reactive<LoginRequest>({
  username: "",
  password: "",
});

const loginRules: FormRules = {
  username: [
    { required: true, message: "Please enter username", trigger: "blur" },
  ],
  password: [
    { required: true, message: "Please enter password", trigger: "blur" },
  ],
};

const handleLogin = async () => {
  if (!loginFormRef.value) return;

  await loginFormRef.value.validate(async (valid) => {
    if (valid) {
      loading.value = true;
      try {
        const success = await authStore.login(loginForm);
        if (success) {
          ElMessage.success("Login successful!");
          await router.push("/");
        } else {
          ElMessage.error("Invalid username or password");
        }
      } catch (error) {
        console.error(error);
        ElMessage.error("Login failed");
      } finally {
        loading.value = false;
      }
    }
  });
};
</script>
