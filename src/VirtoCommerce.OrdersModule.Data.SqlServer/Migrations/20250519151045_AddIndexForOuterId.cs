using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexForOuterId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OrderShipment_OuterId",
                table: "OrderShipment",
                column: "OuterId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPaymentIn_OuterId",
                table: "OrderPaymentIn",
                column: "OuterId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrder_OuterId",
                table: "CustomerOrder",
                column: "OuterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderShipment_OuterId",
                table: "OrderShipment");

            migrationBuilder.DropIndex(
                name: "IX_OrderPaymentIn_OuterId",
                table: "OrderPaymentIn");

            migrationBuilder.DropIndex(
                name: "IX_CustomerOrder_OuterId",
                table: "CustomerOrder");
        }
    }
}
