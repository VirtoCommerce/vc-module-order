using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductSnapshot",
                table: "OrderLineItem");

            migrationBuilder.DropColumn(
                name: "ProductSnapshot",
                table: "OrderConfigurationItem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductSnapshot",
                table: "OrderLineItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductSnapshot",
                table: "OrderConfigurationItem",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
