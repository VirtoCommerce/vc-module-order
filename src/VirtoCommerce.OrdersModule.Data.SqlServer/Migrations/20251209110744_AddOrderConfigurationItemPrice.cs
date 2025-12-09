using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.SqlServer.Migrations
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
                type: "nvarchar(max)",
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
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductSnapshot",
                table: "OrderConfigurationItem",
                type: "nvarchar(max)",
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
