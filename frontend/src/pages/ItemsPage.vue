<template>
  <div class="erp-page">
    <div class="erp-toolbar">
      <el-input
        v-model="searchTerm"
        placeholder="Search by barcode, code, or name..."
        prefix-icon="Search"
        class="erp-search"
        clearable
        @input="debouncedSearch"
      />
      <el-button
        type="primary"
        :loading="dialogLoading && !editingItem"
        @click="openDialog()"
      >
        <el-icon><Plus /></el-icon>
        Add Item
      </el-button>

      <div class="action-group">
        <el-button class="tool-btn" @click="downloadTemplate">
          <el-icon><Download /></el-icon>
          Template
        </el-button>

        <el-button class="tool-btn" @click="openImportDialog">
          <el-icon><Upload /></el-icon>
          Import
        </el-button>
      </div>
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
            <el-button link type="danger" @click="handleDelete(row.id)">
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
      width="1200px"
      :close-on-click-modal="!saveLoading"
    >
      <el-form
        :model="form"
        :rules="rules"
        ref="formRef"
        label-width="150px"
        class="erp-dialog-form"
        v-loading="dialogLoading"
      >
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="Barcode" prop="barcode">
              <el-input
                v-model="form.barcode"
                placeholder="Enter barcode"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Item Code" prop="itemCode">
              <el-input
                v-model="form.itemCode"
                placeholder="Enter item code"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="Item Name" prop="itemName">
          <el-input
            v-model="form.itemName"
            placeholder="Enter item name"
            style="width: 100%"
          />
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
          <el-input
            v-model="form.itemImage"
            placeholder="Enter image URL"
            style="width: 100%"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <div class="dialog-footer">
          <el-button :disabled="saveLoading" @click="dialogVisible = false"
            >Cancel</el-button
          >
          <el-button
            type="primary"
            :loading="saveLoading"
            :disabled="dialogLoading"
            @click="handleSubmit"
            >Save</el-button
          >
        </div>
      </template>
    </el-dialog>

    <el-dialog
      v-model="importDialogVisible"
      title="Import Items"
      width="900px"
      :close-on-click-modal="!importLoading"
    >
      <div class="import-dialog">
        <div class="import-toolbar">
          <el-upload
            accept=".xlsx,.xls,.csv"
            :auto-upload="false"
            :show-file-list="true"
            :limit="1"
            @change="onFileChange"
          >
            <template #trigger>
              <el-button type="primary">Select Excel File</el-button>
            </template>
            <el-button @click="downloadTemplate" class="ml-2"
              >Download Template</el-button
            >
          </el-upload>
          <div class="import-info">Supported formats: .xlsx, .xls, .csv</div>
        </div>

        <el-alert
          v-if="importError"
          type="error"
          show-icon
          :closable="false"
          class="mb-4"
        >
          {{ importError }}
        </el-alert>

        <el-table
          v-if="importPreview.length"
          :data="importPreview"
          border
          style="width: 100%"
          max-height="400"
        >
          <el-table-column prop="barcode" label="Barcode" width="120" />
          <el-table-column prop="itemCode" label="Item Code" width="120" />
          <el-table-column prop="itemName" label="Item Name" width="180" />
          <el-table-column prop="categoryName" label="Category" width="140" />
          <el-table-column prop="brandName" label="Brand" width="140" />
          <el-table-column prop="unitName" label="Unit" width="120" />
          <el-table-column
            prop="purchaseRate"
            label="Purchase Rate"
            width="140"
          />
          <el-table-column prop="costRate" label="Cost Rate" width="140" />
          <el-table-column
            prop="wholesaleRate"
            label="Wholesale Rate"
            width="150"
          />
          <el-table-column prop="retailRate" label="Retail Rate" width="140" />
          <el-table-column prop="mrp" label="MRP" width="120" />
          <el-table-column prop="taxPercentage" label="Tax %" width="100" />
          <el-table-column prop="minimumStock" label="Min Stock" width="120" />
          <el-table-column
            prop="openingStock"
            label="Opening Stock"
            width="140"
          />
          <el-table-column prop="isActive" label="Active" width="100">
            <template #default="{ row }">
              <el-tag :type="row.isActive ? 'success' : 'danger'">
                {{ row.isActive ? "Yes" : "No" }}
              </el-tag>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <template #footer>
        <div class="dialog-footer">
          <el-button @click="importDialogVisible = false">Cancel</el-button>
          <el-button
            type="primary"
            :loading="importLoading"
            :disabled="!importPreview.length || !!importError"
            @click="handleImport"
          >
            Import {{ importPreview.length }} Items
          </el-button>
        </div>
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
import { Plus, Download, Upload } from "@element-plus/icons-vue";
import * as XLSX from "xlsx";
import { useItemStore } from "@/stores/items";
import { useCategoryStore } from "@/stores/categories";
import { useBrandStore } from "@/stores/brands";
import { useUnitStore } from "@/stores/units";
import { itemService } from "@/services/items";
import type { CreateItem, UpdateItem } from "@/services/items";
import type { ImportItemDto } from "@/services/items";

const itemStore = useItemStore();
const categoryStore = useCategoryStore();
const brandStore = useBrandStore();
const unitStore = useUnitStore();

const searchTerm = ref("");
const dialogVisible = ref(false);
const editingItem = ref<any>(null);
const formRef = ref<FormInstance>();
const dialogLoading = ref(false);
const saveLoading = ref(false);

const importDialogVisible = ref(false);
const importLoading = ref(false);
const importError = ref<string>("");
const importPreview = ref<ImportItemDto[]>([]);

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
  dialogLoading.value = true;
  dialogVisible.value = true;
  try {
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
  } finally {
    dialogLoading.value = false;
  }
};

const handleSubmit = async () => {
  if (!formRef.value || saveLoading.value) return;
  const valid = await formRef.value.validate().catch(() => false);
  if (!valid) return;

  saveLoading.value = true;
  try {
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
  } finally {
    saveLoading.value = false;
  }
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

const openImportDialog = async () => {
  importDialogVisible.value = true;
  importError.value = "";
  importPreview.value = [];
  await categoryStore.fetchAll();
  await brandStore.fetchAll();
  await unitStore.fetchAll();
};

const onFileChange = (file: any) => {
  importError.value = "";
  importPreview.value = [];

  const reader = new FileReader();
  reader.onload = (e: any) => {
    try {
      const data = new Uint8Array(e.target.result);
      const workbook = XLSX.read(data, { type: "array" });
      const firstSheet = workbook.Sheets[workbook.SheetNames[0]];
      const json = XLSX.utils.sheet_to_json<Record<string, any>>(firstSheet);

      if (!json.length) {
        importError.value = "Excel file is empty";
        return;
      }

      const normalized = json.map((row) => {
        const entry: any = {};
        Object.keys(row).forEach((key) => {
          const normalizedKey = key.trim().toLowerCase();
          entry[normalizedKey] = row[key];
        });
        return entry;
      });

      const requiredHeaders = [
        "itemcode",
        "itemname",
        "category",
        "brand",
        "unit",
        "purchaserate",
        "costrate",
        "wholesalerate",
        "retailrate",
        "mrp",
        "taxpercentage",
        "minimumstock",
        "openingstock",
      ];

      const missingHeaders = requiredHeaders.filter(
        (header) => normalized[0][header] === undefined,
      );

      if (missingHeaders.length) {
        importError.value = `Missing required columns: ${missingHeaders.join(", ")}`;
        return;
      }

      importPreview.value = normalized.map((row) => ({
        barcode:
          row.barcode === undefined || row.barcode === null
            ? ""
            : String(row.barcode),
        itemCode: String(row.itemcode ?? ""),
        itemName: String(row.itemname ?? ""),
        categoryName: String(row.category ?? ""),
        brandName: String(row.brand ?? ""),
        unitName: String(row.unit ?? ""),
        purchaseRate: Number(row.purchaserate ?? 0),
        costRate: Number(row.costrate ?? 0),
        wholesaleRate: Number(row.wholesalerate ?? 0),
        retailRate: Number(row.retailrate ?? 0),
        mrp: Number(row.mrp ?? 0),
        taxPercentage: Number(row.taxpercentage ?? 0),
        minimumStock: Number(row.minimumstock ?? 0),
        openingStock: Number(row.openingstock ?? 0),
        isActive:
          row.isactive !== undefined
            ? String(row.isactive).toLowerCase() === "true" ||
              String(row.isactive) === "1"
            : true,
      }));
    } catch (err) {
      console.error(err);
      importError.value = "Failed to parse Excel file";
    }
  };

  reader.readAsArrayBuffer(file.raw);
};

const handleImport = async () => {
  if (!importPreview.value.length || importLoading.value) return;

  importLoading.value = true;
  importError.value = "";

  try {
    const response = await itemService.importItems(importPreview.value);

    if (!response.success) {
      throw new Error(response.message || "Import failed");
    }

    const successCount = (response.data ?? []).filter(
      (item: any) => item.success,
    ).length;
    const failedCount = (response.data ?? []).length - successCount;

    ElMessage.success(
      `Imported ${successCount} items successfully${failedCount ? `, ${failedCount} failed` : ""}`,
    );
    importDialogVisible.value = false;
    importPreview.value = [];
    searchTerm.value = "";
    itemStore.pagedItems.pageNumber = 1;
    itemStore.pagedItems.pageSize = 10;
    await itemStore.fetchPaged(1, 10, "");
  } catch (err: any) {
    importError.value = err.message || "Failed to import items";
  } finally {
    importLoading.value = false;
  }
};

const downloadTemplate = () => {
  const headers = [
    "Barcode",
    "ItemCode",
    "ItemName",
    "Category",
    "Brand",
    "Unit",
    "PurchaseRate",
    "CostRate",
    "WholesaleRate",
    "RetailRate",
    "MRP",
    "TaxPercentage",
    "MinimumStock",
    "OpeningStock",
    "IsActive",
  ];

  const sampleRow = [
    "",
    "ITM-001",
    "Sample Item",
    categoryStore.categories[0]?.categoryName ?? "General",
    brandStore.brands[0]?.brandName ?? "Generic",
    unitStore.units[0]?.unitName ?? "Pcs",
    10,
    8,
    12,
    15,
    20,
    5,
    10,
    50,
    "TRUE",
  ];

  const worksheet = XLSX.utils.aoa_to_sheet([headers, sampleRow]);
  const workbook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, "Items");
  XLSX.writeFile(workbook, "items-import-template.xlsx");
};

onMounted(() => {
  loadItems();
});
</script>
