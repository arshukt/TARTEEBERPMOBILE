using FluentMigrator;
using TarteebErp.Shared.Enums;

namespace TarteebErp.Infrastructure.Data.Migrations;

[Migration(001)]
public class CreateInitialTables : Migration
{
    public override void Up()
    {
        Create.Table("Companies")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("CompanyName").AsString(255).NotNullable()
            .WithColumn("Address").AsString(500).Nullable()
            .WithColumn("Mobile").AsString(50).Nullable()
            .WithColumn("Phone").AsString(50).Nullable()
            .WithColumn("Email").AsString(100).Nullable()
            .WithColumn("Website").AsString(255).Nullable()
            .WithColumn("Logo").AsString(500).Nullable()
            .WithColumn("TaxNumber").AsString(100).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("Users")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Username").AsString(50).NotNullable().Unique()
            .WithColumn("PasswordHash").AsString(255).NotNullable()
            .WithColumn("FullName").AsString(100).NotNullable()
            .WithColumn("Mobile").AsString(50).Nullable()
            .WithColumn("Email").AsString(100).Nullable()
            .WithColumn("Role").AsInt32().NotNullable()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("RefreshToken").AsString(500).Nullable()
            .WithColumn("RefreshTokenExpiry").AsDateTime().Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("Customers")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("CustomerCode").AsString(50).NotNullable().Unique()
            .WithColumn("CustomerName").AsString(255).NotNullable()
            .WithColumn("ContactPerson").AsString(100).Nullable()
            .WithColumn("Mobile").AsString(50).Nullable()
            .WithColumn("WhatsApp").AsString(50).Nullable()
            .WithColumn("Email").AsString(100).Nullable()
            .WithColumn("Address").AsString(500).Nullable()
            .WithColumn("City").AsString(100).Nullable()
            .WithColumn("CreditDays").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("CreditLimit").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
            .WithColumn("OpeningBalance").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("Suppliers")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("SupplierCode").AsString(50).NotNullable().Unique()
            .WithColumn("SupplierName").AsString(255).NotNullable()
            .WithColumn("Mobile").AsString(50).Nullable()
            .WithColumn("Email").AsString(100).Nullable()
            .WithColumn("Address").AsString(500).Nullable()
            .WithColumn("OpeningBalance").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("Categories")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("CategoryName").AsString(100).NotNullable()
            .WithColumn("Description").AsString(500).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("Brands")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("BrandName").AsString(100).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("Units")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("UnitName").AsString(50).NotNullable()
            .WithColumn("Symbol").AsString(10).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("Items")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Barcode").AsString(100).Nullable()
            .WithColumn("ItemCode").AsString(50).NotNullable().Unique()
            .WithColumn("ItemName").AsString(255).NotNullable()
            .WithColumn("CategoryId").AsInt32().NotNullable().ForeignKey("FK_Items_Categories_CategoryId", "Categories", "Id")
            .WithColumn("BrandId").AsInt32().NotNullable().ForeignKey("FK_Items_Brands_BrandId", "Brands", "Id")
            .WithColumn("UnitId").AsInt32().NotNullable().ForeignKey("FK_Items_Units_UnitId", "Units", "Id")
            .WithColumn("PurchaseRate").AsDecimal(18, 2).NotNullable()
            .WithColumn("CostRate").AsDecimal(18, 2).NotNullable()
            .WithColumn("WholesaleRate").AsDecimal(18, 2).NotNullable()
            .WithColumn("RetailRate").AsDecimal(18, 2).NotNullable()
            .WithColumn("MRP").AsDecimal(18, 2).NotNullable()
            .WithColumn("TaxPercentage").AsDecimal(5, 2).NotNullable()
            .WithColumn("MinimumStock").AsDecimal(18, 2).NotNullable()
            .WithColumn("OpeningStock").AsDecimal(18, 2).NotNullable()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("ItemImage").AsString(500).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("OpeningStocks")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Date").AsDateTime().NotNullable()
            .WithColumn("Notes").AsString(500).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("OpeningStockDetails")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("OpeningStockId").AsInt32().NotNullable().ForeignKey("FK_OpeningStockDetails_OpeningStocks_OpeningStockId", "OpeningStocks", "Id")
            .WithColumn("ItemId").AsInt32().NotNullable().ForeignKey("FK_OpeningStockDetails_Items_ItemId", "Items", "Id")
            .WithColumn("PurchaseRate").AsDecimal(18, 2).NotNullable()
            .WithColumn("CostRate").AsDecimal(18, 2).NotNullable()
            .WithColumn("RetailRate").AsDecimal(18, 2).NotNullable()
            .WithColumn("WholesaleRate").AsDecimal(18, 2).NotNullable()
            .WithColumn("MRP").AsDecimal(18, 2).NotNullable()
            .WithColumn("Quantity").AsDecimal(18, 2).NotNullable()
            .WithColumn("BatchNumber").AsString(100).Nullable()
            .WithColumn("ExpiryDate").AsDateTime().Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("Purchases")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("SupplierId").AsInt32().NotNullable().ForeignKey("FK_Purchases_Suppliers_SupplierId", "Suppliers", "Id")
            .WithColumn("PurchaseDate").AsDateTime().NotNullable()
            .WithColumn("InvoiceNumber").AsString(100).NotNullable().Unique()
            .WithColumn("TotalAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("Discount").AsDecimal(18, 2).NotNullable()
            .WithColumn("TaxAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("NetAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("PurchaseDetails")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("PurchaseId").AsInt32().NotNullable().ForeignKey("FK_PurchaseDetails_Purchases_PurchaseId", "Purchases", "Id")
            .WithColumn("ItemId").AsInt32().NotNullable().ForeignKey("FK_PurchaseDetails_Items_ItemId", "Items", "Id")
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
            .WithColumn("BatchNumber").AsString(100).Nullable()
            .WithColumn("ExpiryDate").AsDateTime().Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("Sales")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("CustomerId").AsInt32().Nullable().ForeignKey("FK_Sales_Customers_CustomerId", "Customers", "Id")
            .WithColumn("SaleDate").AsDateTime().NotNullable()
            .WithColumn("InvoiceNumber").AsString(100).NotNullable().Unique()
            .WithColumn("TotalAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("Discount").AsDecimal(18, 2).NotNullable()
            .WithColumn("TaxAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("NetAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("PaidAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("DueAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("DueDate").AsDateTime().Nullable()
            .WithColumn("IsCredit").AsBoolean().NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("SaleDetails")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("SaleId").AsInt32().NotNullable().ForeignKey("FK_SaleDetails_Sales_SaleId", "Sales", "Id")
            .WithColumn("ItemId").AsInt32().NotNullable().ForeignKey("FK_SaleDetails_Items_ItemId", "Items", "Id")
            .WithColumn("Quantity").AsDecimal(18, 2).NotNullable()
            .WithColumn("Rate").AsDecimal(18, 2).NotNullable()
            .WithColumn("Discount").AsDecimal(18, 2).NotNullable()
            .WithColumn("TaxPercentage").AsDecimal(5, 2).NotNullable()
            .WithColumn("TaxAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("Total").AsDecimal(18, 2).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("StockTransactions")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("ItemId").AsInt32().NotNullable().ForeignKey("FK_StockTransactions_Items_ItemId", "Items", "Id")
            .WithColumn("TransactionType").AsInt32().NotNullable()
            .WithColumn("QuantityIn").AsDecimal(18, 2).NotNullable()
            .WithColumn("QuantityOut").AsDecimal(18, 2).NotNullable()
            .WithColumn("BalanceAfter").AsDecimal(18, 2).NotNullable()
            .WithColumn("ReferenceId").AsInt32().NotNullable()
            .WithColumn("ReferenceType").AsString(100).NotNullable()
            .WithColumn("Notes").AsString(500).Nullable()
            .WithColumn("TransactionDate").AsDateTime().NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Index("IX_Items_Barcode").OnTable("Items").OnColumn("Barcode");
        Create.Index("IX_StockTransactions_ItemId").OnTable("StockTransactions").OnColumn("ItemId");
        Create.Index("IX_StockTransactions_TransactionDate").OnTable("StockTransactions").OnColumn("TransactionDate");
    }

    public override void Down()
    {
        Delete.Table("StockTransactions");
        Delete.Table("SaleDetails");
        Delete.Table("Sales");
        Delete.Table("PurchaseDetails");
        Delete.Table("Purchases");
        Delete.Table("OpeningStockDetails");
        Delete.Table("OpeningStocks");
        Delete.Table("Items");
        Delete.Table("Units");
        Delete.Table("Brands");
        Delete.Table("Categories");
        Delete.Table("Suppliers");
        Delete.Table("Customers");
        Delete.Table("Users");
        Delete.Table("Companies");
    }
}
