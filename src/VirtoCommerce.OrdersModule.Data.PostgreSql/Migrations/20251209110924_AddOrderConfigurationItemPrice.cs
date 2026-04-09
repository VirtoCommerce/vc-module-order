using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderConfigurationItemPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductSnapshot",
                table: "OrderLineItem",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "OrderConfigurationItem",
                type: "Money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SalePrice",
                table: "OrderConfigurationItem",
                type: "Money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SectionId",
                table: "OrderConfigurationItem",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductSnapshot",
                table: "OrderConfigurationItem",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductSnapshot",
                table: "OrderLineItem");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "OrderConfigurationItem");

            migrationBuilder.DropColumn(
                name: "SalePrice",
                table: "OrderConfigurationItem");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "OrderConfigurationItem");

            migrationBuilder.DropColumn(
                name: "ProductSnapshot",
                table: "OrderConfigurationItem");
        }
    }
}
