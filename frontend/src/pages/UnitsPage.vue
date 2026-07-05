<template>
  <div class="p-6">
    <div class="flex justify-between items-center mb-6">
      <el-input
        v-model="searchTerm"
        placeholder="Search units..."
        prefix-icon="Search"
        style="width: 300px"
        clearable
        @input="debouncedSearch"
      />
      <el-button type="primary" @click="openDialog()">
        <el-icon><Plus /></el-icon>
        Add Unit
      </el-button>
    </div>

    <el-card>
      <el-table
        :data="unitStore.pagedUnits.items"
        v-loading="unitStore.loading"
        stripe
      >
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="unitName" label="Name" width="150" />
        <!-- <el-table-column prop="symbol" label="Symbol" /> -->
        <el-table-column
          label="Actions"
          width="90"
          fixed="right"
          align="center"
        >
          <template #default="{ row }">
            <el-button link type="primary" @click="openDialog(row)">
              Edit
            </el-button>
            <el-button link type="danger" @click="handleDelete(row)">
              Delete
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <el-pagination
        v-model:current-page="unitStore.pagedUnits.pageNumber"
        v-model:page-size="unitStore.pagedUnits.pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="unitStore.pagedUnits.totalCount"
        layout="total, sizes, prev, pager, next, jumper"
        @size-change="loadUnits"
        @current-change="loadUnits"
        class="mt-4"
      />
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="editingUnit ? 'Edit Unit' : 'Add Unit'"
      width="500px"
    >
      <el-form :model="form" :rules="rules" ref="formRef" label-width="120px">
        <el-form-item label="Unit Name" prop="unitName">
          <el-input v-model="form.unitName" placeholder="Enter unit name" />
        </el-form-item>
        <el-form-item label="Symbol" prop="symbol">
          <el-input v-model="form.symbol" placeholder="Enter symbol" />
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
import {
  ElMessage,
  ElMessageBox,
  type FormInstance,
  type FormRules,
} from "element-plus";
import { Plus } from "@element-plus/icons-vue";
import { useUnitStore } from "@/stores/units";
import type { CreateUnit, UpdateUnit } from "@/services/units";

const unitStore = useUnitStore();
const searchTerm = ref("");
const dialogVisible = ref(false);
const editingUnit = ref<any>(null);
const formRef = ref<FormInstance>();

const form = ref<CreateUnit>({
  unitName: "",
  symbol: "",
});

const rules: FormRules = {
  unitName: [
    { required: true, message: "Unit name is required", trigger: "blur" },
  ],
  symbol: [{ required: true, message: "Symbol is required", trigger: "blur" }],
};

let debounceTimer: ReturnType<typeof setTimeout> | null = null;
const debouncedSearch = () => {
  if (debounceTimer) clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    loadUnits();
  }, 300);
};

const loadUnits = () => {
  unitStore.fetchPaged(
    unitStore.pagedUnits.pageNumber,
    unitStore.pagedUnits.pageSize,
    searchTerm.value,
  );
};

const openDialog = (unit?: any) => {
  editingUnit.value = unit || null;
  if (unit) {
    form.value = {
      unitName: unit.unitName,
      symbol: unit.symbol,
    };
  } else {
    form.value = {
      unitName: "",
      symbol: "",
    };
  }
  dialogVisible.value = true;
};

const handleSubmit = async () => {
  if (!formRef.value) return;
  await formRef.value.validate(async (valid) => {
    if (valid) {
      let success = false;
      if (editingUnit.value) {
        success = await unitStore.update({
          id: editingUnit.value.id,
          ...form.value,
        } as UpdateUnit);
      } else {
        success = await unitStore.create(form.value);
      }
      if (success) {
        dialogVisible.value = false;
        loadUnits();
      }
    }
  });
};

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm(
      "Are you sure you want to delete this unit?",
      "Confirm",
      {
        type: "warning",
      },
    );
    const success = await unitStore.remove(id);
    if (success) {
      loadUnits();
    }
  } catch {
    // User cancelled
  }
};

onMounted(() => {
  loadUnits();
});
</script>
