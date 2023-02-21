using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.OrdersModule.Data.SqlServer.Migrations
{
    public partial class AddOrderFulfillmentCenterId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FulfillmentCenterId",
                table: "OrderLineItem",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FulfillmentCenterName",
                table: "OrderLineItem",
                maxLength: 128,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FulfillmentCenterId",
                table: "OrderLineItem");

            migrationBuilder.DropColumn(
                name: "FulfillmentCenterName",
                table: "OrderLineItem");
        }
    }
}
