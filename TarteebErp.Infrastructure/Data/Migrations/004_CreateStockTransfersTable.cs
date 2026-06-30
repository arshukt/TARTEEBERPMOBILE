using FluentMigrator;

namespace TarteebErp.Infrastructure.Data.Migrations;

[Migration(004)]
public class CreateStockTransfersTable : Migration
{
    public override void Up()
    {
        Create.Table("StockTransfers")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("TransferDate").AsDateTime().NotNullable()
            .WithColumn("TransferNumber").AsString(100).NotNullable().Unique()
            .WithColumn("FromLocation").AsString(255).Nullable()
            .WithColumn("ToLocation").AsString(255).Nullable()
            .WithColumn("Notes").AsString(500).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("CreatedBy").AsInt32().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("StockTransferDetails")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("StockTransferId").AsInt32().NotNullable().ForeignKey("FK_StockTransferDetails_StockTransfers_StockTransferId", "StockTransfers", "Id")
            .WithColumn("ItemId").AsInt32().NotNullable().ForeignKey("FK_StockTransferDetails_Items_ItemId", "Items", "Id")
            .WithColumn("Quantity").AsDecimal(18, 2).NotNullable()
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
        Delete.Table("StockTransferDetails");
        Delete.Table("StockTransfers");
    }
}
