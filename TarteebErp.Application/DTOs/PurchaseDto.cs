using TarteebErp.Domain.Entities;

namespace TarteebErp.Application.DTOs;

public class PurchaseDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public List<PurchaseDetailDto> PurchaseDetails { get; set; } = new List<PurchaseDetailDto>();
    public SupplierDto? Supplier { get; set; }
}

public class PurchaseDetailDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal CostRate { get; set; }
    public decimal RetailRate { get; set; }
    public decimal WholesaleRate { get; set; }
    public decimal MRP { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public ItemDto? Item { get; set; }
}

public class CreatePurchaseDto
{
    public int SupplierId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public List<CreatePurchaseDetailDto> PurchaseDetails { get; set; } = new List<CreatePurchaseDetailDto>();
}

public class CreatePurchaseDetailDto
{
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal CostRate { get; set; }
    public decimal RetailRate { get; set; }
    public decimal WholesaleRate { get; set; }
    public decimal MRP { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxPercentage { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class UpdatePurchaseDto : CreatePurchaseDto
{
    public int Id { get; set; }
}
