<template>
  <div class="p-6">
    <div class="flex justify-between items-center mb-6">
      <el-input
        v-model="searchTerm"
        placeholder="Search categories..."
        prefix-icon="Search"
        style="width: 300px"
        clearable
        @input="debouncedSearch"
      />
      <el-button type="primary" @click="openDialog()">
        <el-icon><Plus /></el-icon>
        Add Category
      </el-button>
    </div>

    <el-card>
      <el-table :data="categoryStore.pagedCategories.items" v-loading="categoryStore.loading" stripe>
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="categoryName" label="Category Name" />
        <el-table-column prop="description" label="Description" />
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
        v-model:current-page="categoryStore.pagedCategories.pageNumber"
        v-model:page-size="categoryStore.pagedCategories.pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="categoryStore.pagedCategories.totalCount"
        layout="total, sizes, prev, pager, next, jumper"
        @size-change="loadCategories"
        @current-change="loadCategories"
        class="mt-4"
      />
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="editingCategory ? 'Edit Category' : 'Add Category'"
      width="500px"
    >
      <el-form :model="form" :rules="rules" ref="formRef" label-width="120px">
        <el-form-item label="Category Name" prop="categoryName">
          <el-input v-model="form.categoryName" placeholder="Enter category name" />
        </el-form-item>
        <el-form-item label="Description" prop="description">
          <el-input
            v-model="form.description"
            type="textarea"
            :rows="3"
            placeholder="Enter description"
          />
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
import { useCategoryStore } from "@/stores/categories";
import type { CreateCategory, UpdateCategory } from "@/services/categories";

const categoryStore = useCategoryStore();
const searchTerm = ref("");
const dialogVisible = ref(false);
const editingCategory = ref<any>(null);
const formRef = ref<FormInstance>();

const form = ref<CreateCategory>({
  categoryName: "",
  description: ""
});

const rules: FormRules = {
  categoryName: [
    { required: true, message: "Category name is required", trigger: "blur" }
  ]
};

let debounceTimer: ReturnType<typeof setTimeout> | null = null;
const debouncedSearch = () => {
  if (debounceTimer) clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    loadCategories();
  }, 300);
};

const loadCategories = () => {
  categoryStore.fetchPaged(
    categoryStore.pagedCategories.pageNumber,
    categoryStore.pagedCategories.pageSize,
    searchTerm.value
  );
};

const openDialog = (category?: any) => {
  editingCategory.value = category || null;
  if (category) {
    form.value = {
      categoryName: category.categoryName,
      description: category.description
    };
  } else {
    form.value = {
      categoryName: "",
      description: ""
    };
  }
  dialogVisible.value = true;
};

const handleSubmit = async () => {
  if (!formRef.value) return;
  await formRef.value.validate(async (valid) => {
    if (valid) {
      let success = false;
      if (editingCategory.value) {
        success = await categoryStore.update({
          id: editingCategory.value.id,
          ...form.value
        } as UpdateCategory);
      } else {
        success = await categoryStore.create(form.value);
      }
      if (success) {
        dialogVisible.value = false;
        loadCategories();
      }
    }
  });
};

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm("Are you sure you want to delete this category?", "Confirm", {
      type: "warning"
    });
    const success = await categoryStore.remove(id);
    if (success) {
      loadCategories();
    }
  } catch {
    // User cancelled
  }
};

onMounted(() => {
  loadCategories();
});
</script>
