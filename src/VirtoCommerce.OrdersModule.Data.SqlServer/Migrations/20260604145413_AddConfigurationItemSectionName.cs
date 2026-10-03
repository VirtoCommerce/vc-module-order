using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationItemSectionName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrderConfigurationItem') AND name = 'SectionName')
                    ALTER TABLE [OrderConfigurationItem] ADD [SectionName] nvarchar(256) NULL
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrderConfigurationItem') AND name = 'SectionName')
                    ALTER TABLE [OrderConfigurationItem] DROP COLUMN [SectionName]
                """);
        }
    }
}
