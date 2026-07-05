namespace TarteebErp.Domain.DTOs;

public class CurrentStockReportItemDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class SalesReportItemDto
{
    public DateTime Date { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class PurchasesReportItemDto
{
    public DateTime Date { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class CustomerOutstandingReportItemDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalOutstanding { get; set; }
    public DateTime? LastTransaction { get; set; }
}

public class SupplierOutstandingReportItemDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalOutstanding { get; set; }
    public DateTime? LastTransaction { get; set; }
}
