using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.PostgreSql.Migrations
{
    public partial class AddOrderTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefundId",
                table: "OrderDynamicPropertyObjectValue",
                type: "character varying(128)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CustomerOrder",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrderRefund",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Amount = table.Column<decimal>(type: "Money", nullable: false),
                    ReasonCode = table.Column<string>(type: "text", nullable: true),
                    ReasonMessage = table.Column<string>(type: "text", nullable: true),
                    RejectReasonMessage = table.Column<string>(type: "text", nullable: true),
                    VendorId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CustomerOrderId = table.Column<string>(type: "character varying(128)", nullable: true),
                    PaymentId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Comment = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Sum = table.Column<decimal>(type: "Money", nullable: false),
                    IsCancelled = table.Column<bool>(type: "boolean", nullable: false),
                    CancelledState = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ParentOperationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderRefund", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderRefund_CustomerOrder_CustomerOrderId",
                        column: x => x.CustomerOrderId,
                        principalTable: "CustomerOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderRefund_OrderPaymentIn_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "OrderPaymentIn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderRefundItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    LineItemId = table.Column<string>(type: "character varying(128)", nullable: false),
                    RefundId = table.Column<string>(type: "character varying(128)", nullable: false),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderRefundItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderRefundItem_OrderLineItem_LineItemId",
                        column: x => x.LineItemId,
                        principalTable: "OrderLineItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderRefundItem_OrderRefund_RefundId",
                        column: x => x.RefundId,
                        principalTable: "OrderRefund",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDynamicProperty_ObjectType_RefundId",
                table: "OrderDynamicPropertyObjectValue",
                columns: new[] { "ObjectType", "RefundId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDynamicPropertyObjectValue_RefundId",
                table: "OrderDynamicPropertyObjectValue",
                column: "RefundId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRefund_CustomerOrderId",
                table: "OrderRefund",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRefund_PaymentId",
                table: "OrderRefund",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRefundItem_LineItemId",
                table: "OrderRefundItem",
                column: "LineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRefundItem_RefundId",
                table: "OrderRefundItem",
                column: "RefundId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDynamicPropertyObjectValue_OrderRefund_RefundId",
                table: "OrderDynamicPropertyObjectValue",
                column: "RefundId",
                principalTable: "OrderRefund",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDynamicPropertyObjectValue_OrderRefund_RefundId",
                table: "OrderDynamicPropertyObjectValue");

            migrationBuilder.DropTable(
                name: "OrderRefundItem");

            migrationBuilder.DropTable(
                name: "OrderRefund");

            migrationBuilder.DropIndex(
                name: "IX_OrderDynamicProperty_ObjectType_RefundId",
                table: "OrderDynamicPropertyObjectValue");

            migrationBuilder.DropIndex(
                name: "IX_OrderDynamicPropertyObjectValue_RefundId",
                table: "OrderDynamicPropertyObjectValue");

            migrationBuilder.DropColumn(
                name: "RefundId",
                table: "OrderDynamicPropertyObjectValue");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CustomerOrder");
        }
    }
}
