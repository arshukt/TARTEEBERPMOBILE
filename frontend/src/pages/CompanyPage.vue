<template>
  <div class="p-6">
    <h2 class="text-xl font-bold text-[#025f8b]">Company Settings</h2>

    <el-card>
      <div v-if="companyStore.loading" class="text-center py-10">
        <el-icon class="is-loading" size="40"><Loading /></el-icon>
        <p class="mt-2">Loading...</p>
      </div>
      <template v-else>
        <el-form :model="form" :rules="rules" ref="formRef" label-width="150px">
          <el-form-item label="Company Name" prop="companyName">
            <el-input
              v-model="form.companyName"
              placeholder="Enter company name"
            />
          </el-form-item>
          <el-form-item label="Address" prop="address">
            <el-input
              v-model="form.address"
              type="textarea"
              :rows="2"
              placeholder="Enter address"
            />
          </el-form-item>
          <el-row :gutter="20">
            <el-col :span="12">
              <el-form-item label="Mobile" prop="mobile">
                <el-input
                  v-model="form.mobile"
                  placeholder="Enter mobile number"
                />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="Phone" prop="phone">
                <el-input
                  v-model="form.phone"
                  placeholder="Enter phone number"
                />
              </el-form-item>
            </el-col>
          </el-row>
          <el-row :gutter="20">
            <el-col :span="12">
              <el-form-item label="Email" prop="email">
                <el-input v-model="form.email" placeholder="Enter email" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="Website" prop="website">
                <el-input v-model="form.website" placeholder="Enter website" />
              </el-form-item>
            </el-col>
          </el-row>
          <el-row :gutter="20">
            <el-col :span="12">
              <el-form-item label="Tax Number" prop="taxNumber">
                <el-input
                  v-model="form.taxNumber"
                  placeholder="Enter tax number"
                />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="Logo URL" prop="logo">
                <el-input v-model="form.logo" placeholder="Enter logo URL" />
              </el-form-item>
            </el-col>
          </el-row>
          <el-form-item>
            <el-button
              type="primary"
              @click="handleSubmit"
              :loading="companyStore.loading"
            >
              {{ companyStore.currentCompany ? "Update" : "Create" }} Company
            </el-button>
          </el-form-item>
        </el-form>
      </template>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { type FormInstance, type FormRules } from "element-plus";
import { Loading } from "@element-plus/icons-vue";
import { useCompanyStore } from "@/stores/companies";
import type { CreateCompany, UpdateCompany } from "@/services/companies";

const companyStore = useCompanyStore();
const formRef = ref<FormInstance>();

const form = ref<CreateCompany>({
  companyName: "",
  address: "",
  mobile: "",
  phone: "",
  email: "",
  website: "",
  logo: "",
  taxNumber: "",
});

const rules: FormRules = {
  companyName: [
    { required: true, message: "Company name is required", trigger: "blur" },
  ],
};

const loadCompany = async () => {
  await companyStore.fetchFirst();
  if (companyStore.currentCompany) {
    form.value = {
      companyName: companyStore.currentCompany.companyName,
      address: companyStore.currentCompany.address,
      mobile: companyStore.currentCompany.mobile,
      phone: companyStore.currentCompany.phone,
      email: companyStore.currentCompany.email,
      website: companyStore.currentCompany.website,
      logo: companyStore.currentCompany.logo,
      taxNumber: companyStore.currentCompany.taxNumber,
    };
  }
};

const handleSubmit = async () => {
  if (!formRef.value) return;
  await formRef.value.validate(async (valid) => {
    if (valid) {
      let success = false;
      if (companyStore.currentCompany) {
        success = await companyStore.update({
          id: companyStore.currentCompany.id,
          ...form.value,
        } as UpdateCompany);
      } else {
        success = await companyStore.create(form.value);
      }
      if (success) {
        loadCompany();
      }
    }
  });
};

onMounted(() => {
  loadCompany();
});
</script>
