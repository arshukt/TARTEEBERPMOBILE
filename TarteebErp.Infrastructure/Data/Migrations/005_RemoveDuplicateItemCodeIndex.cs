using FluentMigrator;

namespace TarteebErp.Infrastructure.Data.Migrations;

[Migration(005)]
public class RemoveDuplicateItemCodeIndex : Migration
{
    public override void Up()
    {
        // Only drop if it exists
        Execute.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM pg_indexes 
                    WHERE schemaname = 'public' 
                    AND tablename = 'items' 
                    AND indexname = 'ix_items_itemcode'
                ) THEN
                    DROP INDEX ""IX_Items_ItemCode"";
                END IF;
            END $$;
        ");
    }

    public override void Down()
    {
        // No need to recreate - the unique constraint already handles it
    }
}
