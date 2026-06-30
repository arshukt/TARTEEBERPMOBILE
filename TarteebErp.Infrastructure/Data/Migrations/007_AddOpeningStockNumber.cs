using FluentMigrator;

namespace TarteebErp.Infrastructure.Data.Migrations;

[Migration(007)]
public class AddOpeningStockNumber : Migration
{
    public override void Up()
    {
        Alter.Table("OpeningStocks")
            .AddColumn("OpeningStockNumber").AsString(100).Nullable();

        Execute.Sql(@"
            UPDATE ""OpeningStocks""
            SET ""OpeningStockNumber"" = 'OS-' || LPAD(""Id""::text, 6, '0')
            WHERE ""OpeningStockNumber"" IS NULL OR ""OpeningStockNumber"" = ''");

        Alter.Table("OpeningStocks")
            .AlterColumn("OpeningStockNumber").AsString(100).NotNullable();

        Create.Index("IX_OpeningStocks_OpeningStockNumber")
            .OnTable("OpeningStocks")
            .OnColumn("OpeningStockNumber")
            .Unique();
    }

    public override void Down()
    {
        Delete.Index("IX_OpeningStocks_OpeningStockNumber").OnTable("OpeningStocks");
        Delete.Column("OpeningStockNumber").FromTable("OpeningStocks");
    }
}
