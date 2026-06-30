using TarteebErp.Domain.Entities;

namespace TarteebErp.Application.DTOs;

public class OpeningStockDto
{
    public int Id { get; set; }
    public string OpeningStockNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public List<OpeningStockDetailDto> OpeningStockDetails { get; set; } = new List<OpeningStockDetailDto>();
}

public class OpeningStockDetailDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal CostRate { get; set; }
    public decimal RetailRate { get; set; }
    public decimal WholesaleRate { get; set; }
    public decimal MRP { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public ItemDto? Item { get; set; }
}

public class CreateOpeningStockDto
{
    public string OpeningStockNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public List<CreateOpeningStockDetailDto> OpeningStockDetails { get; set; } = new List<CreateOpeningStockDetailDto>();
}

public class CreateOpeningStockDetailDto
{
    public int ItemId { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal CostRate { get; set; }
    public decimal RetailRate { get; set; }
    public decimal WholesaleRate { get; set; }
    public decimal MRP { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class UpdateOpeningStockDto : CreateOpeningStockDto
{
    public int Id { get; set; }
}
