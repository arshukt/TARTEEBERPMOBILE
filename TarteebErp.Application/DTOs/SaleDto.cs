using TarteebErp.Domain.Entities;

namespace TarteebErp.Application.DTOs;

public class SaleDto
{
    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public DateTime SaleDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCredit { get; set; }
    public List<SaleDetailDto> SaleDetails { get; set; } = new List<SaleDetailDto>();
    public CustomerDto? Customer { get; set; }
}

public class SaleDetailDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public ItemDto? Item { get; set; }
}

public class CreateSaleDto
{
    public int? CustomerId { get; set; }
    public DateTime SaleDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public List<CreateSaleDetailDto> SaleDetails { get; set; } = new List<CreateSaleDetailDto>();
    public decimal PaidAmount { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCredit { get; set; }
}

public class CreateSaleDetailDto
{
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxPercentage { get; set; }
}

public class UpdateSaleDto : CreateSaleDto
{
    public int Id { get; set; }
}
