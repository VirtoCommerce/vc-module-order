using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationItemSectionName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "OrderConfigurationItem" ADD COLUMN IF NOT EXISTS "SectionName" character varying(256) NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "OrderConfigurationItem" DROP COLUMN IF EXISTS "SectionName";
                """);
        }
    }
}
