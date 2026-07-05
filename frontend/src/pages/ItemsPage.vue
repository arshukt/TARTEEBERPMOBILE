<template>
  <div class="p-6">
    <div class="flex justify-between items-center mb-6">
      <el-input
        v-model="searchTerm"
        placeholder="Search by barcode, code, or name..."
        prefix-icon="Search"
        style="width: 300px"
        clearable
        @input="debouncedSearch"
      />
      <el-button type="primary" @click="openDialog()">
        <el-icon><Plus /></el-icon>
        Add Item
      </el-button>
    </div>

    <el-card>
      <el-table
        :data="itemStore.pagedItems.items"
        v-loading="itemStore.loading"
        stripe
      >
        <el-table-column prop="id" label="ID" width="60" />
        <!-- <el-table-column prop="barcode" label="Barcode" width="100" /> -->
        <el-table-column prop="itemCode" label="Item Code" width="100" />
        <el-table-column prop="itemName" label="Item Name" width="150" />
        <el-table-column prop="categoryName" label="Category" width="100" />
        <!-- <el-table-column prop="brandName" label="Brand" width="100" /> -->
        <el-table-column prop="unitName" label="Unit" width="60" />
        <el-table-column prop="retailRate" label="RRP" width="120">
          <template #default="{ row }">{{
            formatCurrency(row.retailRate)
          }}</template>
        </el-table-column>
        <el-table-column prop="isActive" label="Status" width="100">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'danger'">
              {{ row.isActive ? "Active" : "Inactive" }}
            </el-tag>
          </template>
        </el-table-column>
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
        v-model:current-page="itemStore.pagedItems.pageNumber"
        v-model:page-size="itemStore.pagedItems.pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="itemStore.pagedItems.totalCount"
        layout="total, sizes, prev, pager, next, jumper"
        @size-change="loadItems"
        @current-change="loadItems"
        class="mt-4"
      />
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="editingItem ? 'Edit Item' : 'Add Item'"
      width="700px"
    >
      <el-form :model="form" :rules="rules" ref="formRef" label-width="150px">
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="Barcode" prop="barcode">
              <el-input v-model="form.barcode" placeholder="Enter barcode" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Item Code" prop="itemCode">
              <el-input v-model="form.itemCode" placeholder="Enter item code" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="Item Name" prop="itemName">
          <el-input v-model="form.itemName" placeholder="Enter item name" />
        </el-form-item>

        <el-row :gutter="20">
          <el-col :span="8">
            <el-form-item label="Category" prop="categoryId">
              <el-select
                v-model="form.categoryId"
                placeholder="Select category"
                style="width: 100%"
              >
                <el-option
                  v-for="category in categoryStore.categories"
                  :key="category.id"
                  :label="category.categoryName"
                  :value="category.id"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="Brand" prop="brandId">
              <el-select
                v-model="form.brandId"
                placeholder="Select brand"
                style="width: 100%"
              >
                <el-option
                  v-for="brand in brandStore.brands"
                  :key="brand.id"
                  :label="brand.brandName"
                  :value="brand.id"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="Unit" prop="unitId">
              <el-select
                v-model="form.unitId"
                placeholder="Select unit"
                style="width: 100%"
              >
                <el-option
                  v-for="unit in unitStore.units"
                  :key="unit.id"
                  :label="unit.unitName"
                  :value="unit.id"
                />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="20">
          <el-col :span="6">
            <el-form-item label="Purchase Rate" prop="purchaseRate">
              <el-input-number
                v-model="form.purchaseRate"
                :precision="2"
                :step="0.01"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="Cost Rate" prop="costRate">
              <el-input-number
                v-model="form.costRate"
                :precision="2"
                :step="0.01"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="Wholesale Rate" prop="wholesaleRate">
              <el-input-number
                v-model="form.wholesaleRate"
                :precision="2"
                :step="0.01"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="Retail Rate" prop="retailRate">
              <el-input-number
                v-model="form.retailRate"
                :precision="2"
                :step="0.01"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="20">
          <el-col :span="6">
            <el-form-item label="MRP" prop="mrp">
              <el-input-number
                v-model="form.mrp"
                :precision="2"
                :step="0.01"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="Tax %" prop="taxPercentage">
              <el-input-number
                v-model="form.taxPercentage"
                :precision="2"
                :step="0.01"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="Min Stock" prop="minimumStock">
              <el-input-number
                v-model="form.minimumStock"
                :precision="2"
                :step="1"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="Opening Stock" prop="openingStock">
              <el-input-number
                v-model="form.openingStock"
                :precision="2"
                :step="1"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="Active" prop="isActive">
          <el-switch v-model="form.isActive" />
        </el-form-item>

        <el-form-item label="Item Image" prop="itemImage">
          <el-input v-model="form.itemImage" placeholder="Enter image URL" />
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
import { useItemStore } from "@/stores/items";
import { useCategoryStore } from "@/stores/categories";
import { useBrandStore } from "@/stores/brands";
import { useUnitStore } from "@/stores/units";
import type { CreateItem, UpdateItem } from "@/services/items";

const itemStore = useItemStore();
const categoryStore = useCategoryStore();
const brandStore = useBrandStore();
const unitStore = useUnitStore();

const searchTerm = ref("");
const dialogVisible = ref(false);
const editingItem = ref<any>(null);
const formRef = ref<FormInstance>();

const form = ref<CreateItem>({
  barcode: "",
  itemCode: "",
  itemName: "",
  categoryId: 0,
  brandId: 0,
  unitId: 0,
  purchaseRate: 0,
  costRate: 0,
  wholesaleRate: 0,
  retailRate: 0,
  mrp: 0,
  taxPercentage: 0,
  minimumStock: 0,
  openingStock: 0,
  isActive: true,
  itemImage: "",
});

const rules: FormRules = {
  itemCode: [
    { required: true, message: "Item code is required", trigger: "blur" },
  ],
  itemName: [
    { required: true, message: "Item name is required", trigger: "blur" },
  ],
  categoryId: [
    { required: true, message: "Category is required", trigger: "change" },
  ],
  brandId: [
    { required: true, message: "Brand is required", trigger: "change" },
  ],
  unitId: [{ required: true, message: "Unit is required", trigger: "change" }],
  purchaseRate: [
    { required: true, message: "Purchase rate is required", trigger: "blur" },
  ],
  costRate: [
    { required: true, message: "Cost rate is required", trigger: "blur" },
  ],
  wholesaleRate: [
    { required: true, message: "Wholesale rate is required", trigger: "blur" },
  ],
  retailRate: [
    { required: true, message: "Retail rate is required", trigger: "blur" },
  ],
  mrp: [{ required: true, message: "MRP is required", trigger: "blur" }],
  taxPercentage: [
    { required: true, message: "Tax percentage is required", trigger: "blur" },
  ],
  minimumStock: [
    { required: true, message: "Minimum stock is required", trigger: "blur" },
  ],
  openingStock: [
    { required: true, message: "Opening stock is required", trigger: "blur" },
  ],
};

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "QAR",
  }).format(value);
};

let debounceTimer: ReturnType<typeof setTimeout> | null = null;
const debouncedSearch = () => {
  if (debounceTimer) clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    loadItems();
  }, 300);
};

const loadItems = () => {
  itemStore.fetchPaged(
    itemStore.pagedItems.pageNumber,
    itemStore.pagedItems.pageSize,
    searchTerm.value,
  );
};

const openDialog = async (item?: any) => {
  await categoryStore.fetchAll();
  await brandStore.fetchAll();
  await unitStore.fetchAll();

  editingItem.value = item || null;
  if (item) {
    form.value = {
      barcode: item.barcode,
      itemCode: item.itemCode,
      itemName: item.itemName,
      categoryId: item.categoryId,
      brandId: item.brandId,
      unitId: item.unitId,
      purchaseRate: item.purchaseRate,
      costRate: item.costRate,
      wholesaleRate: item.wholesaleRate,
      retailRate: item.retailRate,
      mrp: item.mrp,
      taxPercentage: item.taxPercentage,
      minimumStock: item.minimumStock,
      openingStock: item.openingStock,
      isActive: item.isActive,
      itemImage: item.itemImage,
    };
  } else {
    form.value = {
      barcode: "",
      itemCode: "",
      itemName: "",
      categoryId: 0,
      brandId: 0,
      unitId: 0,
      purchaseRate: 0,
      costRate: 0,
      wholesaleRate: 0,
      retailRate: 0,
      mrp: 0,
      taxPercentage: 0,
      minimumStock: 0,
      openingStock: 0,
      isActive: true,
      itemImage: "",
    };
  }
  dialogVisible.value = true;
};

const handleSubmit = async () => {
  if (!formRef.value) return;
  await formRef.value.validate(async (valid) => {
    if (valid) {
      let success = false;
      if (editingItem.value) {
        success = await itemStore.update({
          id: editingItem.value.id,
          ...form.value,
        } as UpdateItem);
      } else {
        success = await itemStore.create(form.value);
      }
      if (success) {
        dialogVisible.value = false;
        loadItems();
      }
    }
  });
};

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm(
      "Are you sure you want to delete this item?",
      "Confirm",
      {
        type: "warning",
      },
    );
    const success = await itemStore.remove(id);
    if (success) {
      loadItems();
    }
  } catch {
    // User cancelled
  }
};

onMounted(() => {
  loadItems();
});
</script>
