using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressMiddleName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "OrderAddress",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "OrderAddress");
        }
    }
}
