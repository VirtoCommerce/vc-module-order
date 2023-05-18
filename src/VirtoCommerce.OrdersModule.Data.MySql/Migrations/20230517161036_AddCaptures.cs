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
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomerOrder",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StoreId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StoreName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChannelId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmployeeId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmployeeName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubscriptionId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubscriptionNumber = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPrototype = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SubTotalWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ShippingTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ShippingTotalWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PaymentTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PaymentTotalWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FeeWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FeeTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FeeTotalWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    HandlingTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    HandlingTotalWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountTotalWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LanguageCode = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaxPercentRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ShoppingCartId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PurchaseOrderNumber = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
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
                    Sum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_CustomerOrder", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderLineItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PriceId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PriceWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmountWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FeeWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxPercentRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CatalogId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CategoryId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sku = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Comment = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsReccuring = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ImageUrl = table.Column<string>(type: "varchar(1028)", maxLength: 1028, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsGift = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ShippingMethodCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FulfillmentLocationCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WeightUnit = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MeasureUnit = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Height = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    TaxType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsCancelled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CancelledDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancelReason = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OuterId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FulfillmentCenterId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FulfillmentCenterName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VendorId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerOrderId = table.Column<string>(type: "varchar(128)", nullable: true)
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
                    table.PrimaryKey("PK_OrderLineItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLineItem_CustomerOrder_CustomerOrderId",
                        column: x => x.CustomerOrderId,
                        principalTable: "CustomerOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderShipment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FulfillmentCenterId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FulfillmentCenterName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmployeeId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmployeeName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShipmentMethodCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShipmentMethodOption = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VolumetricWeight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    WeightUnit = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MeasureUnit = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Height = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    TaxType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PriceWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmountWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FeeWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxPercentRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TrackingNumber = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrackingUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeliveryDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    VendorId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerOrderId = table.Column<string>(type: "varchar(128)", nullable: false)
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
                    Sum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_OrderShipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderShipment_CustomerOrder_CustomerOrderId",
                        column: x => x.CustomerOrderId,
                        principalTable: "CustomerOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderPaymentIn",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IncomingDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Purpose = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GatewayCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AuthorizedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CapturedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    VoidedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TaxType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PriceWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmountWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxPercentRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    VendorId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerOrderId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShipmentId = table.Column<string>(type: "varchar(128)", nullable: true)
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
                    Sum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_OrderPaymentIn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderPaymentIn_CustomerOrder_CustomerOrderId",
                        column: x => x.CustomerOrderId,
                        principalTable: "CustomerOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderPaymentIn_OrderShipment_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "OrderShipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderShipmentPackage",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BarCode = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PackageType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WeightUnit = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MeasureUnit = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Height = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ShipmentId = table.Column<string>(type: "varchar(128)", nullable: false)
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
                    table.PrimaryKey("PK_OrderShipmentPackage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderShipmentPackage_OrderShipment_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "OrderShipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderAddress",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AddressType = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Organization = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryCode = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PostalCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Line1 = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Line2 = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegionId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegionName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OuterId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerOrderId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShipmentId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentInId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderAddress_CustomerOrder_CustomerOrderId",
                        column: x => x.CustomerOrderId,
                        principalTable: "CustomerOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderAddress_OrderPaymentIn_PaymentInId",
                        column: x => x.PaymentInId,
                        principalTable: "OrderPaymentIn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderAddress_OrderShipment_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "OrderShipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderCapture",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    Sum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                name: "OrderDiscount",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PromotionId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PromotionDescription = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmountWithTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CouponCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CouponInvalidDescription = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerOrderId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShipmentId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LineItemId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentInId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDiscount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDiscount_CustomerOrder_CustomerOrderId",
                        column: x => x.CustomerOrderId,
                        principalTable: "CustomerOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDiscount_OrderLineItem_LineItemId",
                        column: x => x.LineItemId,
                        principalTable: "OrderLineItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDiscount_OrderPaymentIn_PaymentInId",
                        column: x => x.PaymentInId,
                        principalTable: "OrderPaymentIn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDiscount_OrderShipment_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "OrderShipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderFeeDetail",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FeeId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Currency = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerOrderId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShipmentId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LineItemId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentInId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderPaymentGatewayTransaction",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsProcessed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ProcessError = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProcessAttemptCount = table.Column<int>(type: "int", nullable: false),
                    RequestData = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseData = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GatewayIpAddress = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Note = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentInId = table.Column<string>(type: "varchar(128)", nullable: false)
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
                    table.PrimaryKey("PK_OrderPaymentGatewayTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderPaymentGatewayTransaction_OrderPaymentIn_PaymentInId",
                        column: x => x.PaymentInId,
                        principalTable: "OrderPaymentIn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderRefund",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReasonCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReasonMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RejectReasonMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    Sum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_OrderRefund", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderRefund_CustomerOrder_CustomerOrderId",
                        column: x => x.CustomerOrderId,
                        principalTable: "CustomerOrder",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderRefund_OrderPaymentIn_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "OrderPaymentIn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderTaxDetail",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerOrderId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShipmentId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LineItemId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentInId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTaxDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderTaxDetail_CustomerOrder_CustomerOrderId",
                        column: x => x.CustomerOrderId,
                        principalTable: "CustomerOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderTaxDetail_OrderLineItem_LineItemId",
                        column: x => x.LineItemId,
                        principalTable: "OrderLineItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderTaxDetail_OrderPaymentIn_PaymentInId",
                        column: x => x.PaymentInId,
                        principalTable: "OrderPaymentIn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderTaxDetail_OrderShipment_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "OrderShipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderShipmentItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BarCode = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    LineItemId = table.Column<string>(type: "varchar(128)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShipmentId = table.Column<string>(type: "varchar(128)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShipmentPackageId = table.Column<string>(type: "varchar(128)", nullable: true)
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
                    table.PrimaryKey("PK_OrderShipmentItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderShipmentItem_OrderLineItem_LineItemId",
                        column: x => x.LineItemId,
                        principalTable: "OrderLineItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderShipmentItem_OrderShipment_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "OrderShipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderShipmentItem_OrderShipmentPackage_ShipmentPackageId",
                        column: x => x.ShipmentPackageId,
                        principalTable: "OrderShipmentPackage",
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

            migrationBuilder.CreateTable(
                name: "OrderDynamicPropertyObjectValue",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerOrderId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentInId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShipmentId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LineItemId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RefundId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CaptureId = table.Column<string>(type: "varchar(128)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ObjectType = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ObjectId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Locale = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValueType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortTextValue = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LongTextValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DecimalValue = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    IntegerValue = table.Column<int>(type: "int", nullable: true),
                    BooleanValue = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    DateTimeValue = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PropertyId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DictionaryItemId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PropertyName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDynamicPropertyObjectValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDynamicPropertyObjectValue_CustomerOrder_CustomerOrderId",
                        column: x => x.CustomerOrderId,
                        principalTable: "CustomerOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDynamicPropertyObjectValue_OrderCapture_CaptureId",
                        column: x => x.CaptureId,
                        principalTable: "OrderCapture",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderDynamicPropertyObjectValue_OrderLineItem_LineItemId",
                        column: x => x.LineItemId,
                        principalTable: "OrderLineItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDynamicPropertyObjectValue_OrderPaymentIn_PaymentInId",
                        column: x => x.PaymentInId,
                        principalTable: "OrderPaymentIn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDynamicPropertyObjectValue_OrderRefund_RefundId",
                        column: x => x.RefundId,
                        principalTable: "OrderRefund",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDynamicPropertyObjectValue_OrderShipment_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "OrderShipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderRefundItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    LineItemId = table.Column<string>(type: "varchar(128)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RefundId = table.Column<string>(type: "varchar(128)", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAddress_CustomerOrderId",
                table: "OrderAddress",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAddress_PaymentInId",
                table: "OrderAddress",
                column: "PaymentInId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAddress_ShipmentId",
                table: "OrderAddress",
                column: "ShipmentId");

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

            migrationBuilder.CreateIndex(
                name: "IX_OrderDiscount_CustomerOrderId",
                table: "OrderDiscount",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDiscount_LineItemId",
                table: "OrderDiscount",
                column: "LineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDiscount_PaymentInId",
                table: "OrderDiscount",
                column: "PaymentInId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDiscount_ShipmentId",
                table: "OrderDiscount",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDynamicProperty_ObjectType_CustomerOrderId",
                table: "OrderDynamicPropertyObjectValue",
                columns: new[] { "ObjectType", "CustomerOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDynamicProperty_ObjectType_LineItemId",
                table: "OrderDynamicPropertyObjectValue",
                columns: new[] { "ObjectType", "LineItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDynamicProperty_ObjectType_ObjectId",
                table: "OrderDynamicPropertyObjectValue",
                columns: new[] { "ObjectType", "ObjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDynamicProperty_ObjectType_PaymentInId",
                table: "OrderDynamicPropertyObjectValue",
                columns: new[] { "ObjectType", "PaymentInId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDynamicProperty_ObjectType_RefundId",
                table: "OrderDynamicPropertyObjectValue",
                columns: new[] { "ObjectType", "RefundId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDynamicProperty_ObjectType_ShipmentId",
                table: "OrderDynamicPropertyObjectValue",
                columns: new[] { "ObjectType", "ShipmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDynamicPropertyObjectValue_CaptureId",
                table: "OrderDynamicPropertyObjectValue",
                column: "CaptureId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDynamicPropertyObjectValue_CustomerOrderId",
                table: "OrderDynamicPropertyObjectValue",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDynamicPropertyObjectValue_LineItemId",
                table: "OrderDynamicPropertyObjectValue",
                column: "LineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDynamicPropertyObjectValue_PaymentInId",
                table: "OrderDynamicPropertyObjectValue",
                column: "PaymentInId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDynamicPropertyObjectValue_RefundId",
                table: "OrderDynamicPropertyObjectValue",
                column: "RefundId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDynamicPropertyObjectValue_ShipmentId",
                table: "OrderDynamicPropertyObjectValue",
                column: "ShipmentId");

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

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineItem_CustomerOrderId",
                table: "OrderLineItem",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPaymentGatewayTransaction_PaymentInId",
                table: "OrderPaymentGatewayTransaction",
                column: "PaymentInId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPaymentIn_CustomerOrderId",
                table: "OrderPaymentIn",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPaymentIn_ShipmentId",
                table: "OrderPaymentIn",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRefund_CustomerOrderId",
                table: "OrderRefund",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRefund_PaymentId",
                table: "OrderRefund",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRefund_TransactionId_CustomerOrderId",
                table: "OrderRefund",
                columns: new[] { "TransactionId", "CustomerOrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderRefundItem_LineItemId",
                table: "OrderRefundItem",
                column: "LineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRefundItem_RefundId",
                table: "OrderRefundItem",
                column: "RefundId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderShipment_CustomerOrderId",
                table: "OrderShipment",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderShipmentItem_LineItemId",
                table: "OrderShipmentItem",
                column: "LineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderShipmentItem_ShipmentId",
                table: "OrderShipmentItem",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderShipmentItem_ShipmentPackageId",
                table: "OrderShipmentItem",
                column: "ShipmentPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderShipmentPackage_ShipmentId",
                table: "OrderShipmentPackage",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTaxDetail_CustomerOrderId",
                table: "OrderTaxDetail",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTaxDetail_LineItemId",
                table: "OrderTaxDetail",
                column: "LineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTaxDetail_PaymentInId",
                table: "OrderTaxDetail",
                column: "PaymentInId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTaxDetail_ShipmentId",
                table: "OrderTaxDetail",
                column: "ShipmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderAddress");

            migrationBuilder.DropTable(
                name: "OrderCaptureItem");

            migrationBuilder.DropTable(
                name: "OrderDiscount");

            migrationBuilder.DropTable(
                name: "OrderDynamicPropertyObjectValue");

            migrationBuilder.DropTable(
                name: "OrderFeeDetail");

            migrationBuilder.DropTable(
                name: "OrderPaymentGatewayTransaction");

            migrationBuilder.DropTable(
                name: "OrderRefundItem");

            migrationBuilder.DropTable(
                name: "OrderShipmentItem");

            migrationBuilder.DropTable(
                name: "OrderTaxDetail");

            migrationBuilder.DropTable(
                name: "OrderCapture");

            migrationBuilder.DropTable(
                name: "OrderRefund");

            migrationBuilder.DropTable(
                name: "OrderShipmentPackage");

            migrationBuilder.DropTable(
                name: "OrderLineItem");

            migrationBuilder.DropTable(
                name: "OrderPaymentIn");

            migrationBuilder.DropTable(
                name: "OrderShipment");

            migrationBuilder.DropTable(
                name: "CustomerOrder");
        }
    }
}
