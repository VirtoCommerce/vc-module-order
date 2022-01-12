using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.OrdersModule.Data.Migrations
{
    public partial class AddFees : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Fee",
                table: "CustomerOrder",
                type: "Money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeTotal",
                table: "CustomerOrder",
                type: "Money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeTotalWithTax",
                table: "CustomerOrder",
                type: "Money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeWithTax",
                table: "CustomerOrder",
                type: "Money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(@"
                UPDATE [CustomerOrder] SET FeeTotal = HandlingTotal, FeeTotalWithTax = HandlingTotalWithTax
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fee",
                table: "CustomerOrder");

            migrationBuilder.DropColumn(
                name: "FeeTotal",
                table: "CustomerOrder");

            migrationBuilder.DropColumn(
                name: "FeeTotalWithTax",
                table: "CustomerOrder");

            migrationBuilder.DropColumn(
                name: "FeeWithTax",
                table: "CustomerOrder");
        }
    }
}
