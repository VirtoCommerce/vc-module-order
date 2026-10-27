using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerOrderCustomerIdCreatedDateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrder_CustomerId_CreatedDate",
                table: "CustomerOrder",
                columns: new[] { "CustomerId", "CreatedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerOrder_CustomerId_CreatedDate",
                table: "CustomerOrder");
        }
    }
}
