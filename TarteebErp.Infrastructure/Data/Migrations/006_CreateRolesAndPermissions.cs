using FluentMigrator;

namespace TarteebErp.Infrastructure.Data.Migrations;

[Migration(006)]
public class CreateRolesAndPermissions : Migration
{
    public override void Up()
    {
        Create.Table("Roles")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("RoleName").AsString(100).NotNullable()
            .WithColumn("Description").AsString(500).Nullable()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime)
            .WithColumn("CreatedBy").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        Create.Table("RolePermissions")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("RoleId").AsInt32().NotNullable().ForeignKey("FK_RolePermissions_Roles_RoleId", "Roles", "Id")
            .WithColumn("PermissionKey").AsString(100).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime)
            .WithColumn("CreatedBy").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedBy").AsInt32().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("DeletedAt").AsDateTime().Nullable()
            .WithColumn("DeletedBy").AsInt32().Nullable();

        // Seed Default Roles
        Insert.IntoTable("Roles").Row(new { RoleName = "Admin", Description = "System Administrator" });
        Insert.IntoTable("Roles").Row(new { RoleName = "Manager", Description = "Manager" });
        Insert.IntoTable("Roles").Row(new { RoleName = "Cashier", Description = "Cashier" });
        Insert.IntoTable("Roles").Row(new { RoleName = "Salesman", Description = "Salesman" });

        // Seed ALL permissions for Admin role (RoleId = 1)
        var adminPermissions = new[]
        {
            "Dashboard", "Company", "Customers", "Suppliers", "Categories",
            "Brands", "Units", "Items", "OpeningStocks", "Purchases",
            "PurchaseReturns", "Sales", "SaleReturns", "StockAdjustments",
            "StockTransfers", "StockLedger", "Payments", "Reports",
            "Roles", "Users"
        };
        foreach (var perm in adminPermissions)
        {
            Insert.IntoTable("RolePermissions").Row(new { RoleId = 1, PermissionKey = perm });
        }

        // Add RoleId to Users and migrate old Role Enum values to new RoleId
        Alter.Table("Users").AddColumn("RoleId").AsInt32().Nullable();
        Execute.Sql(@"UPDATE ""Users"" SET ""RoleId"" = ""Role"""); // Assuming enum IDs 1=Admin, 2=Manager, 3=Cashier, 4=Salesman match new Roles Id

        Alter.Table("Users").AlterColumn("RoleId").AsInt32().NotNullable().ForeignKey("FK_Users_Roles_RoleId", "Roles", "Id");
        Delete.Column("Role").FromTable("Users");
    }

    public override void Down()
    {
        Alter.Table("Users").AddColumn("Role").AsInt32().NotNullable().WithDefaultValue(1);
        Execute.Sql(@"UPDATE ""Users"" SET ""Role"" = ""RoleId""");
        Delete.ForeignKey("FK_Users_Roles_RoleId").OnTable("Users");
        Delete.Column("RoleId").FromTable("Users");

        Delete.Table("RolePermissions");
        Delete.Table("Roles");
    }
}
