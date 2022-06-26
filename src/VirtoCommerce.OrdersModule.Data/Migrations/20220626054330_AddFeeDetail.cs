using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.Migrations
{
    public partial class AddFeeDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderFeeDetail",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FeeId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Amount = table.Column<decimal>(type: "Money", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerOrderId = table.Column<string>(type: "nvarchar(128)", nullable: true),
                    ShipmentId = table.Column<string>(type: "nvarchar(128)", nullable: true),
                    LineItemId = table.Column<string>(type: "nvarchar(128)", nullable: true),
                    PaymentInId = table.Column<string>(type: "nvarchar(128)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderFeeDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderFeeDetail_CustomerOrder_CustomerOrderId",
                        column: x => x.CustomerOrderId,
                        principalTable: "CustomerOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderFeeDetail_OrderLineItem_LineItemId",
                        column: x => x.LineItemId,
                        principalTable: "OrderLineItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderFeeDetail_OrderPaymentIn_PaymentInId",
                        column: x => x.PaymentInId,
                        principalTable: "OrderPaymentIn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderFeeDetail_OrderShipment_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "OrderShipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderFeeDetail_CustomerOrderId",
                table: "OrderFeeDetail",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderFeeDetail_LineItemId",
                table: "OrderFeeDetail",
                column: "LineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderFeeDetail_PaymentInId",
                table: "OrderFeeDetail",
                column: "PaymentInId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderFeeDetail_ShipmentId",
                table: "OrderFeeDetail",
                column: "ShipmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderFeeDetail");
        }
    }
}
