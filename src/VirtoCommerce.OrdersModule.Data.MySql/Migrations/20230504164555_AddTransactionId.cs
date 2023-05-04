using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.MySql.Migrations
{
    public partial class AddTransactionId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "OrderRefund",
                type: "varchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("update orderrefund set TransactionId = Id;");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRefund_TransactionId_CustomerOrderId",
                table: "OrderRefund",
                columns: new[] { "TransactionId", "CustomerOrderId" },
                unique: true);
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
