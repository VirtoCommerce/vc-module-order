using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.PostgreSql.Migrations
{
    public partial class ChangeOrderRefundDeleteBehavior : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderRefund_CustomerOrder_CustomerOrderId",
                table: "OrderRefund");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderRefund_OrderPaymentIn_PaymentId",
                table: "OrderRefund");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderRefund_CustomerOrder_CustomerOrderId",
                table: "OrderRefund",
                column: "CustomerOrderId",
                principalTable: "CustomerOrder",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderRefund_OrderPaymentIn_PaymentId",
                table: "OrderRefund",
                column: "PaymentId",
                principalTable: "OrderPaymentIn",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderRefund_CustomerOrder_CustomerOrderId",
                table: "OrderRefund");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderRefund_OrderPaymentIn_PaymentId",
                table: "OrderRefund");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderRefund_CustomerOrder_CustomerOrderId",
                table: "OrderRefund",
                column: "CustomerOrderId",
                principalTable: "CustomerOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderRefund_OrderPaymentIn_PaymentId",
                table: "OrderRefund",
                column: "PaymentId",
                principalTable: "OrderPaymentIn",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
