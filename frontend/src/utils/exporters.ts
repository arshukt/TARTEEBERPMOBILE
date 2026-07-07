import jsPDF from 'jspdf'
import 'jspdf-autotable'
import * as XLSX from 'xlsx'
import { itemService } from '@/services/items'
import { purchaseService } from '@/services/purchases'
import { saleService } from '@/services/sales'
import { useItemStore } from '@/stores/items'
import type { SalesReportItem } from '@/services/reports'
import type { PurchasesReportItem } from '@/services/reports'
import type { Purchase, PurchaseDetail } from '@/services/purchases'
import type { Sale, SaleDetail } from '@/services/sales'

async function resolveItemNames(details: Array<{ itemId: number; item?: { itemName?: string } }>): Promise<Map<number, string>> {
  const itemStore = useItemStore()
  await itemStore.fetchAll()

  const itemNameById = new Map<number, string>()
  itemStore.items.forEach(item => itemNameById.set(item.id, item.itemName))

  const missingItemIds = new Set<number>()
  details.forEach(detail => {
    if (!itemNameById.has(detail.itemId)) {
      missingItemIds.add(detail.itemId)
    }
  })

  if (missingItemIds.size > 0) {
    const itemResponses = await Promise.allSettled(
      Array.from(missingItemIds).map(id => itemService.getById(id)),
    )
    itemResponses.forEach((result, index) => {
      const id = Array.from(missingItemIds)[index]!
      if (result.status === 'fulfilled' && result.value.success && result.value.data) {
        itemNameById.set(id, result.value.data.itemName)
      }
    })
  }

  return itemNameById
}

export async function buildSalesExportRows(rows: SalesReportItem[]): Promise<any[]> {
  if (!rows.length) {
    return []
  }

  const saleIds = Array.from(
    new Set(rows.map(row => row.saleId).filter((id): id is number => typeof id === 'number')),
  )

  const salesById = new Map<number, Sale>()
  if (saleIds.length) {
    const responses = await Promise.allSettled(
      saleIds.map(id => saleService.getById(id)),
    )
    responses.forEach((result, index) => {
      if (result.status === 'fulfilled' && result.value.success && result.value.data) {
        salesById.set(saleIds[index]!, result.value.data)
      }
    })
  }

  const allDetails: Array<{ itemId: number; item?: { itemName?: string } }> = []
  salesById.forEach(sale => {
    (sale.saleDetails || []).forEach((detail: SaleDetail) => {
      allDetails.push({ itemId: detail.itemId, item: detail.item })
    })
  })

  const itemNameById = await resolveItemNames(allDetails)

  return rows.flatMap(row => {
    const sale = salesById.get(row.saleId!)
    const details = sale?.saleDetails || []
    if (!details.length) {
      return [
        {
          Date: row.date,
          Invoice: row.invoiceNumber,
          Customer: row.customer,
          Item: 'No detail available',
          Quantity: 0,
          Rate: 0,
          Discount: 0,
          'Tax %': 0,
          Total: row.total,
        },
      ]
    }

    return details.map(detail => {
      const itemName = detail.item?.itemName || itemNameById.get(detail.itemId) || detail.itemId
      return {
        Date: row.date,
        Invoice: row.invoiceNumber,
        Customer: row.customer,
        Item: itemName,
        Quantity: detail.quantity,
        Rate: detail.rate,
        Discount: detail.discount,
        'Tax %': detail.taxPercentage,
        Total: detail.quantity * detail.rate - detail.discount,
      }
    })
  })
}

export async function buildPurchaseExportRows(rows: PurchasesReportItem[]): Promise<any[]> {
  if (!rows.length) {
    return []
  }

  const purchaseIds = Array.from(
    new Set(rows.map(row => row.purchaseId).filter((id): id is number => typeof id === 'number')),
  )

  const purchasesById = new Map<number, Purchase>()
  if (purchaseIds.length) {
    const responses = await Promise.allSettled(
      purchaseIds.map(id => purchaseService.getById(id)),
    )
    responses.forEach((result, index) => {
      if (result.status === 'fulfilled' && result.value.success && result.value.data) {
        purchasesById.set(purchaseIds[index]!, result.value.data)
      }
    })
  }

  const allDetails: Array<{ itemId: number; item?: { itemName?: string } }> = []
  purchasesById.forEach(purchase => {
    (purchase.purchaseDetails || []).forEach((detail: PurchaseDetail) => {
      allDetails.push({ itemId: detail.itemId, item: detail.item })
    })
  })

  const itemNameById = await resolveItemNames(allDetails)

  return rows.flatMap(row => {
    const purchase = purchasesById.get(row.purchaseId!)
    const details = purchase?.purchaseDetails || []
    if (!details.length) {
      return [
        {
          Date: row.date,
          Invoice: row.invoiceNumber,
          Supplier: row.supplier,
          Item: 'No detail available',
          Quantity: 0,
          Rate: 0,
          Discount: 0,
          'Tax %': 0,
          Total: row.total,
        },
      ]
    }

    return details.map(detail => {
      const itemName = detail.item?.itemName || itemNameById.get(detail.itemId) || detail.itemId
      return {
        Date: row.date,
        Invoice: row.invoiceNumber,
        Supplier: row.supplier,
        Item: itemName,
        Quantity: detail.quantity,
        Rate: detail.purchaseRate,
        Discount: detail.discount,
        'Tax %': detail.taxPercentage,
        Total: detail.quantity * detail.purchaseRate - detail.discount,
      }
    })
  })
}

export async function exportSalesToPdf(rows: SalesReportItem[], filename = 'sales-report.pdf') {
  if (!rows.length) {
    return
  }

  const data = await buildSalesExportRows(rows)
  const doc = new jsPDF()

  doc.setFontSize(16)
  doc.text('Sales Report', 14, 15)

  const tableColumn = [
    'Date',
    'Invoice',
    'Customer',
    'Item',
    'Qty',
    'Rate',
    'Discount',
    'Tax %',
    'Total',
  ]
  const tableRows: any[] = []

  for (const row of data) {
    const tableRow = [
      row.Date,
      row.Invoice,
      row.Customer,
      row.Item,
      row.Quantity,
      row.Rate,
      row.Discount,
      row['Tax %'],
      row.Total,
    ]
    tableRows.push(tableRow)
  }

  ;(doc as any).autoTable({
    head: [tableColumn],
    body: tableRows,
    startY: 22,
    styles: { fontSize: 8 },
    headStyles: { fontSize: 8 },
  })

  doc.save(filename)
}

export async function exportSalesToExcel(rows: SalesReportItem[], filename = 'sales-report.xlsx') {
  if (!rows.length) {
    return
  }

  const data = await buildSalesExportRows(rows)
  const worksheet = XLSX.utils.json_to_sheet(data)
  const workbook = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(workbook, worksheet, 'Sales')
  XLSX.writeFile(workbook, filename)
}

export async function exportPurchasesToPdf(rows: PurchasesReportItem[], filename = 'purchases-report.pdf') {
  if (!rows.length) {
    return
  }

  const data = await buildPurchaseExportRows(rows)
  const doc = new jsPDF()

  doc.setFontSize(16)
  doc.text('Purchase Report', 14, 15)

  const tableColumn = [
    'Date',
    'Invoice',
    'Supplier',
    'Item',
    'Qty',
    'Rate',
    'Discount',
    'Tax %',
    'Total',
  ]
  const tableRows: any[] = []

  for (const row of data) {
    const tableRow = [
      row.Date,
      row.Invoice,
      row.Supplier,
      row.Item,
      row.Quantity,
      row.Rate,
      row.Discount,
      row['Tax %'],
      row.Total,
    ]
    tableRows.push(tableRow)
  }

  ;(doc as any).autoTable({
    head: [tableColumn],
    body: tableRows,
    startY: 22,
    styles: { fontSize: 8 },
    headStyles: { fontSize: 8 },
  })

  doc.save(filename)
}

export async function exportPurchasesToExcel(rows: PurchasesReportItem[], filename = 'purchases-report.xlsx') {
  if (!rows.length) {
    return
  }

  const data = await buildPurchaseExportRows(rows)
  const worksheet = XLSX.utils.json_to_sheet(data)
  const workbook = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(workbook, worksheet, 'Purchases')
  XLSX.writeFile(workbook, filename)
}
