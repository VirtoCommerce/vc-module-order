using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.SqlServer.Migrations
{
    public partial class AddTransactionId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "OrderRefund",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE OrderRefund SET TransactionId = Id");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRefund_TransactionId_CustomerOrderId",
                table: "OrderRefund",
                columns: new[] { "TransactionId", "CustomerOrderId" },
                unique: true,
                filter: "[CustomerOrderId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderRefund_TransactionId_CustomerOrderId",
                table: "OrderRefund");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "OrderRefund");
        }
    }
}
