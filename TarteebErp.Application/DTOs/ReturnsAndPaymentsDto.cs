namespace TarteebErp.Application.DTOs;

public class PurchaseReturnDto
{
    public int Id { get; set; }
    public int PurchaseId { get; set; }
    public DateTime ReturnDate { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseReturnDetailDto> PurchaseReturnDetails { get; set; } = new List<PurchaseReturnDetailDto>();
}

public class PurchaseReturnDetailDto
{
    public int Id { get; set; }
    public int PurchaseReturnId { get; set; }
    public int PurchaseDetailId { get; set; }
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
    public string? Reason { get; set; }
}

public class CreatePurchaseReturnDto
{
    public int PurchaseId { get; set; }
    public DateTime ReturnDate { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<CreatePurchaseReturnDetailDto> PurchaseReturnDetails { get; set; } = new List<CreatePurchaseReturnDetailDto>();
}

public class CreatePurchaseReturnDetailDto
{
    public int PurchaseDetailId { get; set; }
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal CostRate { get; set; }
    public decimal RetailRate { get; set; }
    public decimal WholesaleRate { get; set; }
    public decimal MRP { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxPercentage { get; set; }
    public string? Reason { get; set; }
}

public class UpdatePurchaseReturnDto : CreatePurchaseReturnDto
{
    public int Id { get; set; }
}

public class SaleReturnDto
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public DateTime ReturnDate { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string? Notes { get; set; }
    public List<SaleReturnDetailDto> SaleReturnDetails { get; set; } = new List<SaleReturnDetailDto>();
}

public class SaleReturnDetailDto
{
    public int Id { get; set; }
    public int SaleReturnId { get; set; }
    public int SaleDetailId { get; set; }
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string? Reason { get; set; }
}

public class CreateSaleReturnDto
{
    public int SaleId { get; set; }
    public DateTime ReturnDate { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<CreateSaleReturnDetailDto> SaleReturnDetails { get; set; } = new List<CreateSaleReturnDetailDto>();
}

public class CreateSaleReturnDetailDto
{
    public int SaleDetailId { get; set; }
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxPercentage { get; set; }
    public string? Reason { get; set; }
}

public class UpdateSaleReturnDto : CreateSaleReturnDto
{
    public int Id { get; set; }
}

public class StockAdjustmentDto
{
    public int Id { get; set; }
    public DateTime AdjustmentDate { get; set; }
    public string AdjustmentNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<StockAdjustmentDetailDto> StockAdjustmentDetails { get; set; } = new List<StockAdjustmentDetailDto>();
}

public class StockAdjustmentDetailDto
{
    public int Id { get; set; }
    public int StockAdjustmentId { get; set; }
    public int ItemId { get; set; }
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public string? Reason { get; set; }
}

public class CreateStockAdjustmentDto
{
    public DateTime AdjustmentDate { get; set; }
    public string AdjustmentNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<CreateStockAdjustmentDetailDto> StockAdjustmentDetails { get; set; } = new List<CreateStockAdjustmentDetailDto>();
}

public class CreateStockAdjustmentDetailDto
{
    public int ItemId { get; set; }
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public string? Reason { get; set; }
}

public class UpdateStockAdjustmentDto : CreateStockAdjustmentDto
{
    public int Id { get; set; }
}

public class PaymentDto
{
    public int Id { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public int PaymentType { get; set; }
    public int PartyType { get; set; }
    public int PartyId { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class CreatePaymentDto
{
    public DateTime PaymentDate { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public int PaymentType { get; set; }
    public int PartyType { get; set; }
    public int PartyId { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePaymentDto : CreatePaymentDto
{
    public int Id { get; set; }
}

public class StockTransferDto
{
    public int Id { get; set; }
    public DateTime TransferDate { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public string? FromLocation { get; set; }
    public string? ToLocation { get; set; }
    public string? Notes { get; set; }
    public List<StockTransferDetailDto> StockTransferDetails { get; set; } = new List<StockTransferDetailDto>();
}

public class StockTransferDetailDto
{
    public int Id { get; set; }
    public int StockTransferId { get; set; }
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
}

public class CreateStockTransferDto
{
    public DateTime TransferDate { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public string? FromLocation { get; set; }
    public string? ToLocation { get; set; }
    public string? Notes { get; set; }
    public List<CreateStockTransferDetailDto> StockTransferDetails { get; set; } = new List<CreateStockTransferDetailDto>();
}

public class CreateStockTransferDetailDto
{
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
}

public class UpdateStockTransferDto : CreateStockTransferDto
{
    public int Id { get; set; }
}
