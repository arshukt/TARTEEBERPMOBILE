<template>
  <div class="p-6">
    <div class="flex justify-between items-center mb-6">
      <el-input
        v-model="searchTerm"
        placeholder="Search brands..."
        prefix-icon="Search"
        style="width: 300px"
        clearable
        @input="debouncedSearch"
      />
      <el-button type="primary" @click="openDialog()">
        <el-icon><Plus /></el-icon>
        Add Brand
      </el-button>
    </div>

    <el-card>
      <el-table :data="brandStore.pagedBrands.items" v-loading="brandStore.loading" stripe>
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="brandName" label="Brand Name" />
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
        v-model:current-page="brandStore.pagedBrands.pageNumber"
        v-model:page-size="brandStore.pagedBrands.pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="brandStore.pagedBrands.totalCount"
        layout="total, sizes, prev, pager, next, jumper"
        @size-change="loadBrands"
        @current-change="loadBrands"
        class="mt-4"
      />
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="editingBrand ? 'Edit Brand' : 'Add Brand'"
      width="500px"
    >
      <el-form :model="form" :rules="rules" ref="formRef" label-width="120px">
        <el-form-item label="Brand Name" prop="brandName">
          <el-input v-model="form.brandName" placeholder="Enter brand name" />
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
import { ElMessageBox, type FormInstance, type FormRules } from "element-plus";
import { Plus } from "@element-plus/icons-vue";
import { useBrandStore } from "@/stores/brands";
import type { CreateBrand, UpdateBrand } from "@/services/brands";

const brandStore = useBrandStore();
const searchTerm = ref("");
const dialogVisible = ref(false);
const editingBrand = ref<any>(null);
const formRef = ref<FormInstance>();

const form = ref<CreateBrand>({
  brandName: ""
});

const rules: FormRules = {
  brandName: [
    { required: true, message: "Brand name is required", trigger: "blur" }
  ]
};

let debounceTimer: ReturnType<typeof setTimeout> | null = null;
const debouncedSearch = () => {
  if (debounceTimer) clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    loadBrands();
  }, 300);
};

const loadBrands = () => {
  brandStore.fetchPaged(
    brandStore.pagedBrands.pageNumber,
    brandStore.pagedBrands.pageSize,
    searchTerm.value
  );
};

const openDialog = (brand?: any) => {
  editingBrand.value = brand || null;
  if (brand) {
    form.value = {
      brandName: brand.brandName
    };
  } else {
    form.value = {
      brandName: ""
    };
  }
  dialogVisible.value = true;
};

const handleSubmit = async () => {
  if (!formRef.value) return;
  await formRef.value.validate(async (valid) => {
    if (valid) {
      let success = false;
      if (editingBrand.value) {
        success = await brandStore.update({
          id: editingBrand.value.id,
          ...form.value
        } as UpdateBrand);
      } else {
        success = await brandStore.create(form.value);
      }
      if (success) {
        dialogVisible.value = false;
        loadBrands();
      }
    }
  });
};

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm("Are you sure you want to delete this brand?", "Confirm", {
      type: "warning"
    });
    const success = await brandStore.remove(id);
    if (success) {
      loadBrands();
    }
  } catch {
    // User cancelled
  }
};

onMounted(() => {
  loadBrands();
});
</script>
