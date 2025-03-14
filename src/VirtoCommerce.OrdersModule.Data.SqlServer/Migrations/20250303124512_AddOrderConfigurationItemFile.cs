using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderConfigurationItemFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderConfigurationItemFile",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2083)", maxLength: 2083, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    ConfigurationItemId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderConfigurationItemFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderConfigurationItemFile_OrderConfigurationItem_ConfigurationItemId",
                        column: x => x.ConfigurationItemId,
                        principalTable: "OrderConfigurationItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderConfigurationItemFile_ConfigurationItemId",
                table: "OrderConfigurationItemFile",
                column: "ConfigurationItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderConfigurationItemFile");
        }
    }
}
