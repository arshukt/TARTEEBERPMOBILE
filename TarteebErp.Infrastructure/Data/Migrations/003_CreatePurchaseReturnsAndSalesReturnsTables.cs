using FluentMigrator;

namespace TarteebErp.Infrastructure.Data.Migrations;

[Migration(003)]
public class CreatePurchaseReturnsAndSalesReturnsTables : Migration
{
    public override void Up()
    {
        Create.Table("PurchaseReturns")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("PurchaseId").AsInt32().NotNullable().ForeignKey("FK_PurchaseReturns_Purchases_PurchaseId", "Purchases", "Id")
            .WithColumn("ReturnDate").AsDateTime().NotNullable()
            .WithColumn("ReturnNumber").AsString(100).NotNullable().Unique()
            .WithColumn("TotalAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("Discount").AsDecimal(18, 2).NotNullable()
            .WithColumn("TaxAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("NetAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("Notes").AsString(500).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("PurchaseReturnDetails")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("PurchaseReturnId").AsInt32().NotNullable().ForeignKey("FK_PurchaseReturnDetails_PurchaseReturns_PurchaseReturnId", "PurchaseReturns", "Id")
            .WithColumn("PurchaseDetailId").AsInt32().NotNullable().ForeignKey("FK_PurchaseReturnDetails_PurchaseDetails_PurchaseDetailId", "PurchaseDetails", "Id")
            .WithColumn("ItemId").AsInt32().NotNullable().ForeignKey("FK_PurchaseReturnDetails_Items_ItemId", "Items", "Id")
            .WithColumn("Quantity").AsDecimal(18, 2).NotNullable()
            .WithColumn("PurchaseRate").AsDecimal(18, 2).NotNullable()
            .WithColumn("CostRate").AsDecimal(18, 2).NotNullable()
            .WithColumn("RetailRate").AsDecimal(18, 2).NotNullable()
            .WithColumn("WholesaleRate").AsDecimal(18, 2).NotNullable()
            .WithColumn("MRP").AsDecimal(18, 2).NotNullable()
            .WithColumn("Discount").AsDecimal(18, 2).NotNullable()
            .WithColumn("TaxPercentage").AsDecimal(5, 2).NotNullable()
            .WithColumn("TaxAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("Total").AsDecimal(18, 2).NotNullable()
            .WithColumn("Reason").AsString(500).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("SalesReturns")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("SaleId").AsInt32().NotNullable().ForeignKey("FK_SalesReturns_Sales_SaleId", "Sales", "Id")
            .WithColumn("ReturnDate").AsDateTime().NotNullable()
            .WithColumn("ReturnNumber").AsString(100).NotNullable().Unique()
            .WithColumn("TotalAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("Discount").AsDecimal(18, 2).NotNullable()
            .WithColumn("TaxAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("NetAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("Notes").AsString(500).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("SaleReturnDetails")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("SaleReturnId").AsInt32().NotNullable().ForeignKey("FK_SaleReturnDetails_SalesReturns_SaleReturnId", "SalesReturns", "Id")
            .WithColumn("SaleDetailId").AsInt32().NotNullable().ForeignKey("FK_SaleReturnDetails_SaleDetails_SaleDetailId", "SaleDetails", "Id")
            .WithColumn("ItemId").AsInt32().NotNullable().ForeignKey("FK_SaleReturnDetails_Items_ItemId", "Items", "Id")
            .WithColumn("Quantity").AsDecimal(18, 2).NotNullable()
            .WithColumn("Rate").AsDecimal(18, 2).NotNullable()
            .WithColumn("Discount").AsDecimal(18, 2).NotNullable()
            .WithColumn("TaxPercentage").AsDecimal(5, 2).NotNullable()
            .WithColumn("TaxAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("Total").AsDecimal(18, 2).NotNullable()
            .WithColumn("Reason").AsString(500).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("StockAdjustments")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("AdjustmentDate").AsDateTime().NotNullable()
            .WithColumn("AdjustmentNumber").AsString(100).NotNullable().Unique()
            .WithColumn("Notes").AsString(500).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("StockAdjustmentDetails")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("StockAdjustmentId").AsInt32().NotNullable().ForeignKey("FK_StockAdjustmentDetails_StockAdjustments_StockAdjustmentId", "StockAdjustments", "Id")
            .WithColumn("ItemId").AsInt32().NotNullable().ForeignKey("FK_StockAdjustmentDetails_Items_ItemId", "Items", "Id")
            .WithColumn("QuantityIn").AsDecimal(18, 2).NotNullable()
            .WithColumn("QuantityOut").AsDecimal(18, 2).NotNullable()
            .WithColumn("Reason").AsString(500).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("Payments")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("PaymentDate").AsDateTime().NotNullable()
            .WithColumn("PaymentNumber").AsString(100).NotNullable().Unique()
            .WithColumn("PaymentType").AsInt32().NotNullable()
            .WithColumn("PartyType").AsInt32().NotNullable()
            .WithColumn("PartyId").AsInt32().NotNullable()
            .WithColumn("Amount").AsDecimal(18, 2).NotNullable()
            .WithColumn("PaymentMethod").AsString(100).Nullable()
            .WithColumn("ReferenceNumber").AsString(100).Nullable()
            .WithColumn("Notes").AsString(500).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();
    }

    public override void Down()
    {
        Delete.Table("Payments");
        Delete.Table("StockAdjustmentDetails");
        Delete.Table("StockAdjustments");
        Delete.Table("SaleReturnDetails");
        Delete.Table("SalesReturns");
        Delete.Table("PurchaseReturnDetails");
        Delete.Table("PurchaseReturns");
    }
}
