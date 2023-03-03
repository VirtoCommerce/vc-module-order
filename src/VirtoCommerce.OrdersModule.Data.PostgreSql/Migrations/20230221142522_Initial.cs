using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.PostgreSql.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerOrder",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StoreId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StoreName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ChannelId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OrganizationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OrganizationName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    EmployeeId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    EmployeeName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SubscriptionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SubscriptionNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsPrototype = table.Column<bool>(type: "boolean", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "Money", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "Money", nullable: false),
                    Total = table.Column<decimal>(type: "Money", nullable: false),
                    SubTotal = table.Column<decimal>(type: "Money", nullable: false),
                    SubTotalWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    ShippingTotal = table.Column<decimal>(type: "Money", nullable: false),
                    ShippingTotalWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    PaymentTotal = table.Column<decimal>(type: "Money", nullable: false),
                    PaymentTotalWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    Fee = table.Column<decimal>(type: "Money", nullable: false),
                    FeeWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    FeeTotal = table.Column<decimal>(type: "Money", nullable: false),
                    FeeTotalWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    HandlingTotal = table.Column<decimal>(type: "Money", nullable: false),
                    HandlingTotalWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    DiscountTotal = table.Column<decimal>(type: "Money", nullable: false),
                    DiscountTotalWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    TaxPercentRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ShoppingCartId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PurchaseOrderNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_CustomerOrder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderLineItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PriceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Price = table.Column<decimal>(type: "Money", nullable: false),
                    PriceWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "Money", nullable: false),
                    DiscountAmountWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    Fee = table.Column<decimal>(type: "Money", nullable: false),
                    FeeWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "Money", nullable: false),
                    TaxPercentRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CatalogId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CategoryId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Sku = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProductType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Comment = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    IsReccuring = table.Column<bool>(type: "boolean", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(1028)", maxLength: 1028, nullable: true),
                    IsGift = table.Column<bool>(type: "boolean", nullable: false),
                    ShippingMethodCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FulfillmentLocationCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    WeightUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    MeasureUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Height = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Length = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Width = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    TaxType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsCancelled = table.Column<bool>(type: "boolean", nullable: false),
                    CancelledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FulfillmentCenterId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FulfillmentCenterName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VendorId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CustomerOrderId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "OrderShipment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OrganizationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OrganizationName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FulfillmentCenterId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FulfillmentCenterName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    EmployeeId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    EmployeeName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ShipmentMethodCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ShipmentMethodOption = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    VolumetricWeight = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    WeightUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    MeasureUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Height = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Length = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Width = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    TaxType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Price = table.Column<decimal>(type: "Money", nullable: false),
                    PriceWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "Money", nullable: false),
                    DiscountAmountWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    Total = table.Column<decimal>(type: "Money", nullable: false),
                    TotalWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    Fee = table.Column<decimal>(type: "Money", nullable: false),
                    FeeWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "Money", nullable: false),
                    TaxPercentRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TrackingNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TrackingUrl = table.Column<string>(type: "text", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VendorId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CustomerOrderId = table.Column<string>(type: "character varying(128)", nullable: false),
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
                    table.PrimaryKey("PK_OrderShipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderShipment_CustomerOrder_CustomerOrderId",
                        column: x => x.CustomerOrderId,
                        principalTable: "CustomerOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderPaymentIn",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OrganizationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OrganizationName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IncomingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Purpose = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    GatewayCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    AuthorizedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CapturedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VoidedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TaxType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Price = table.Column<decimal>(type: "Money", nullable: false),
                    PriceWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "Money", nullable: false),
                    DiscountAmountWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    Total = table.Column<decimal>(type: "Money", nullable: false),
                    TotalWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "Money", nullable: false),
                    TaxPercentRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    VendorId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CustomerOrderId = table.Column<string>(type: "character varying(128)", nullable: true),
                    ShipmentId = table.Column<string>(type: "character varying(128)", nullable: true),
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
                });

            migrationBuilder.CreateTable(
                name: "OrderShipmentPackage",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BarCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PackageType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    WeightUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    MeasureUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Height = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Length = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Width = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ShipmentId = table.Column<string>(type: "character varying(128)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "OrderAddress",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    AddressType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Organization = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    CountryName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    City = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Line1 = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Line2 = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    RegionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RegionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FirstName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CustomerOrderId = table.Column<string>(type: "character varying(128)", nullable: true),
                    ShipmentId = table.Column<string>(type: "character varying(128)", nullable: true),
                    PaymentInId = table.Column<string>(type: "character varying(128)", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "OrderDiscount",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PromotionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PromotionDescription = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "Money", nullable: false),
                    DiscountAmountWithTax = table.Column<decimal>(type: "Money", nullable: false),
                    CouponCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CouponInvalidDescription = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CustomerOrderId = table.Column<string>(type: "character varying(128)", nullable: true),
                    ShipmentId = table.Column<string>(type: "character varying(128)", nullable: true),
                    LineItemId = table.Column<string>(type: "character varying(128)", nullable: true),
                    PaymentInId = table.Column<string>(type: "character varying(128)", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "OrderDynamicPropertyObjectValue",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CustomerOrderId = table.Column<string>(type: "character varying(128)", nullable: true),
                    PaymentInId = table.Column<string>(type: "character varying(128)", nullable: true),
                    ShipmentId = table.Column<string>(type: "character varying(128)", nullable: true),
                    LineItemId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ObjectType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ObjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Locale = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ValueType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ShortTextValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    LongTextValue = table.Column<string>(type: "text", nullable: true),
                    DecimalValue = table.Column<decimal>(type: "numeric(18,5)", nullable: true),
                    IntegerValue = table.Column<int>(type: "integer", nullable: true),
                    BooleanValue = table.Column<bool>(type: "boolean", nullable: true),
                    DateTimeValue = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PropertyId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DictionaryItemId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PropertyName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDynamicPropertyObjectValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDynamicPropertyObjectValue_CustomerOrder_CustomerOrder~",
                        column: x => x.CustomerOrderId,
                        principalTable: "CustomerOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        name: "FK_OrderDynamicPropertyObjectValue_OrderShipment_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "OrderShipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderFeeDetail",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FeeId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Currency = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Amount = table.Column<decimal>(type: "Money", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CustomerOrderId = table.Column<string>(type: "character varying(128)", nullable: true),
                    ShipmentId = table.Column<string>(type: "character varying(128)", nullable: true),
                    LineItemId = table.Column<string>(type: "character varying(128)", nullable: true),
                    PaymentInId = table.Column<string>(type: "character varying(128)", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "OrderPaymentGatewayTransaction",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Amount = table.Column<decimal>(type: "Money", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessError = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ProcessAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    RequestData = table.Column<string>(type: "text", nullable: true),
                    ResponseData = table.Column<string>(type: "text", nullable: true),
                    ResponseCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    GatewayIpAddress = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Note = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    PaymentInId = table.Column<string>(type: "character varying(128)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "OrderTaxDetail",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "Money", nullable: false),
                    Name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CustomerOrderId = table.Column<string>(type: "character varying(128)", nullable: true),
                    ShipmentId = table.Column<string>(type: "character varying(128)", nullable: true),
                    LineItemId = table.Column<string>(type: "character varying(128)", nullable: true),
                    PaymentInId = table.Column<string>(type: "character varying(128)", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "OrderShipmentItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BarCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    LineItemId = table.Column<string>(type: "character varying(128)", nullable: false),
                    ShipmentId = table.Column<string>(type: "character varying(128)", nullable: false),
                    ShipmentPackageId = table.Column<string>(type: "character varying(128)", nullable: true),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
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
                });

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
                name: "IX_OrderDynamicProperty_ObjectType_ShipmentId",
                table: "OrderDynamicPropertyObjectValue",
                columns: new[] { "ObjectType", "ShipmentId" });

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
                name: "OrderDiscount");

            migrationBuilder.DropTable(
                name: "OrderDynamicPropertyObjectValue");

            migrationBuilder.DropTable(
                name: "OrderFeeDetail");

            migrationBuilder.DropTable(
                name: "OrderPaymentGatewayTransaction");

            migrationBuilder.DropTable(
                name: "OrderShipmentItem");

            migrationBuilder.DropTable(
                name: "OrderTaxDetail");

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
