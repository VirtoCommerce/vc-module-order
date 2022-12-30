using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.Migrations
{
    public partial class AddVendorId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VendorId",
                table: "OrderShipment",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorId",
                table: "OrderPaymentIn",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "OrderShipment");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "OrderPaymentIn");
        }
    }
}
