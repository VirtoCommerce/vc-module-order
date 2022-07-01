using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.Migrations
{
    public partial class AddFeeFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Fee",
                table: "OrderShipment",
                type: "Money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeWithTax",
                table: "OrderShipment",
                type: "Money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Fee",
                table: "OrderLineItem",
                type: "Money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeWithTax",
                table: "OrderLineItem",
                type: "Money",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fee",
                table: "OrderShipment");

            migrationBuilder.DropColumn(
                name: "FeeWithTax",
                table: "OrderShipment");

            migrationBuilder.DropColumn(
                name: "Fee",
                table: "OrderLineItem");

            migrationBuilder.DropColumn(
                name: "FeeWithTax",
                table: "OrderLineItem");
        }
    }
}
