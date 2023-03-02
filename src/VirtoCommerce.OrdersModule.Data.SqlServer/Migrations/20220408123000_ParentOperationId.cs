using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.SqlServer.Migrations
{
    public partial class ParentOperationId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentOperationId",
                table: "OrderShipment",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentOperationId",
                table: "OrderPaymentIn",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentOperationId",
                table: "CustomerOrder",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentOperationId",
                table: "OrderShipment");

            migrationBuilder.DropColumn(
                name: "ParentOperationId",
                table: "OrderPaymentIn");

            migrationBuilder.DropColumn(
                name: "ParentOperationId",
                table: "CustomerOrder");
        }
    }
}
