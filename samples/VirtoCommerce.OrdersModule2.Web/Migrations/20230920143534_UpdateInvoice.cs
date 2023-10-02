using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule2.Web.Migrations
{
    public partial class UpdateInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelledState",
                table: "OrderInvoice",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentOperationId",
                table: "OrderInvoice",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledState",
                table: "OrderInvoice");

            migrationBuilder.DropColumn(
                name: "ParentOperationId",
                table: "OrderInvoice");
        }
    }
}
