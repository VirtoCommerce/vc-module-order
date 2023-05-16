using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.MySql.Migrations
{
    public partial class AddCaptures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CaptureId",
                table: "OrderDynamicPropertyObjectValue",
                type: "varchar(128)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RowVersion",
                table: "CustomerOrder",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp(6)",
                oldRowVersion: true,
                oldNullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.CreateTable(
                name: "OrderCapture",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "Money(65,30)", nullable: false),
                    VendorId = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerOrderId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Number = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsApproved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Status = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Comment = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sum = table.Column<decimal>(type: "Money(65,30)", nullable: false),
                    IsCancelled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CancelledState = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CancelledDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancelReason = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OuterId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentOperationId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderCaptureItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    LineItemId = table.Column<string>(type: "varchar(128)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CaptureId = table.Column<string>(type: "varchar(128)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OuterId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                unique: true);

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

            migrationBuilder.AlterColumn<DateTime>(
                name: "RowVersion",
                table: "CustomerOrder",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp(6)",
                oldRowVersion: true,
                oldNullable: true)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);
        }
    }
}
