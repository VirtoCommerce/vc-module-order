using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.OrdersModule.Data.Migrations
{
    public partial class AddCancelledStatae : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelledState",
                table: "OrderShipment",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledState",
                table: "OrderPaymentIn",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledState",
                table: "CustomerOrder",
                maxLength: 32,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE [OrderPaymentIn] SET CancelledState = 'Completed' WHERE IsCancelled = 1
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledState",
                table: "OrderShipment");

            migrationBuilder.DropColumn(
                name: "CancelledState",
                table: "OrderPaymentIn");

            migrationBuilder.DropColumn(
                name: "CancelledState",
                table: "CustomerOrder");
        }
    }
}
