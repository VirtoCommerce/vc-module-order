using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.SqlServer.Migrations
{
    public partial class AddCaptures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CaptureId",
                table: "OrderDynamicPropertyObjectValue",
                type: "nvarchar(128)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrderCapture",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Amount = table.Column<decimal>(type: "Money", nullable: false),
                    VendorId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CustomerOrderId = table.Column<string>(type: "nvarchar(128)", nullable: true),
                    PaymentId = table.Column<string>(type: "nvarchar(128)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Number = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Sum = table.Column<decimal>(type: "Money", nullable: false),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    CancelledState = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    OuterId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ParentOperationId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderCapture", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderCapture_CustomerOrder_CustomerOrderId",
                        column: x => x.CustomerOrderId,
                        principalTable: "CustomerOrder",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderCapture_OrderPaymentIn_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "OrderPaymentIn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderCaptureItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    LineItemId = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    CaptureId = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    OuterId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderCaptureItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderCaptureItem_OrderCapture_CaptureId",
                        column: x => x.CaptureId,
                        principalTable: "OrderCapture",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderCaptureItem_OrderLineItem_LineItemId",
                        column: x => x.LineItemId,
                        principalTable: "OrderLineItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDynamicPropertyObjectValue_CaptureId",
                table: "OrderDynamicPropertyObjectValue",
                column: "CaptureId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCapture_CustomerOrderId",
                table: "OrderCapture",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCapture_PaymentId",
                table: "OrderCapture",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCapture_TransactionId_CustomerOrderId",
                table: "OrderCapture",
                columns: new[] { "TransactionId", "CustomerOrderId" },
                unique: true,
                filter: "[CustomerOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCaptureItem_CaptureId",
                table: "OrderCaptureItem",
                column: "CaptureId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCaptureItem_LineItemId",
                table: "OrderCaptureItem",
                column: "LineItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDynamicPropertyObjectValue_OrderCapture_CaptureId",
                table: "OrderDynamicPropertyObjectValue",
                column: "CaptureId",
                principalTable: "OrderCapture",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDynamicPropertyObjectValue_OrderCapture_CaptureId",
                table: "OrderDynamicPropertyObjectValue");

            migrationBuilder.DropTable(
                name: "OrderCaptureItem");

            migrationBuilder.DropTable(
                name: "OrderCapture");

            migrationBuilder.DropIndex(
                name: "IX_OrderDynamicPropertyObjectValue_CaptureId",
                table: "OrderDynamicPropertyObjectValue");

            migrationBuilder.DropColumn(
                name: "CaptureId",
                table: "OrderDynamicPropertyObjectValue");
        }
    }
}
