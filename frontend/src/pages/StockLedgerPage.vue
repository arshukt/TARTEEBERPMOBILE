<template>
  <div class="p-6">
    <div class="flex justify-between items-center mb-6 flex-wrap gap-4">
      <div class="flex gap-4 items-center">
        <el-input
          v-model="searchTerm"
          placeholder="Search stock ledger..."
          prefix-icon="Search"
          style="width: 300px"
          clearable
          @input="debouncedSearch"
        />
        <el-select
          v-model="selectedItemId"
          placeholder="Filter by item"
          clearable
          style="width: 250px"
        >
          <el-option
            v-for="item in itemStore.items"
            :key="item.id"
            :label="item.itemName"
            :value="item.id"
          />
        </el-select>
      </div>
    </div>

    <el-card>
      <el-table
        :data="stockTransactionStore.pagedStockTransactions.items"
        v-loading="stockTransactionStore.loading"
        stripe
      >
        <el-table-column prop="id" label="ID" width="60" />
        <el-table-column prop="transactionDate" label="Date" width="100">
          <template #default="{ row }">
            {{ formatDate(row.transactionDate) }}
          </template>
        </el-table-column>
        <el-table-column
          prop="transactionTypeName"
          label="Transaction Type"
          width="150"
        />
        <el-table-column prop="quantityIn" label="Qty In" width="80" />
        <el-table-column prop="quantityOut" label="Qty Out" width="80" />
        <el-table-column prop="balanceAfter" label="Balance" width="80" />
        <!-- <el-table-column prop="referenceType" label="Reference" width="150" /> -->
        <!-- <el-table-column label="Notes" prop="notes" /> -->
      </el-table>
      <el-pagination
        v-model:current-page="
          stockTransactionStore.pagedStockTransactions.pageNumber
        "
        v-model:page-size="
          stockTransactionStore.pagedStockTransactions.pageSize
        "
        :page-sizes="[10, 20, 50, 100]"
        :total="stockTransactionStore.pagedStockTransactions.totalCount"
        layout="total, sizes, prev, pager, next, jumper"
        @size-change="loadStockTransactions"
        @current-change="loadStockTransactions"
        class="mt-4"
      />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useStockTransactionStore } from "@/stores/stockTransactions";
import { useItemStore } from "@/stores/items";

const stockTransactionStore = useStockTransactionStore();
const itemStore = useItemStore();

const searchTerm = ref("");
const selectedItemId = ref<number | null>(null);

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString();
};

let debounceTimer: ReturnType<typeof setTimeout> | null = null;
const debouncedSearch = () => {
  if (debounceTimer) clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    loadStockTransactions();
  }, 300);
};

const loadStockTransactions = () => {
  stockTransactionStore.fetchPaged(
    stockTransactionStore.pagedStockTransactions.pageNumber,
    stockTransactionStore.pagedStockTransactions.pageSize,
    searchTerm.value,
    selectedItemId.value || undefined,
  );
};

onMounted(async () => {
  await itemStore.fetchAll();
  loadStockTransactions();
});
</script>
