using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.SqlServer.Migrations
{
    public partial class RenameIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_ObjectType_ShipmentId",
                table: "OrderDynamicPropertyObjectValue",
                newName: "IX_OrderDynamicProperty_ObjectType_ShipmentId");

            migrationBuilder.RenameIndex(
                name: "IX_ObjectType_PaymentInId",
                table: "OrderDynamicPropertyObjectValue",
                newName: "IX_OrderDynamicProperty_ObjectType_PaymentInId");

            migrationBuilder.RenameIndex(
                name: "IX_ObjectType_ObjectId",
                table: "OrderDynamicPropertyObjectValue",
                newName: "IX_OrderDynamicProperty_ObjectType_ObjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ObjectType_LineItemId",
                table: "OrderDynamicPropertyObjectValue",
                newName: "IX_OrderDynamicProperty_ObjectType_LineItemId");

            migrationBuilder.RenameIndex(
                name: "IX_ObjectType_CustomerOrderId",
                table: "OrderDynamicPropertyObjectValue",
                newName: "IX_OrderDynamicProperty_ObjectType_CustomerOrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_OrderDynamicProperty_ObjectType_ShipmentId",
                table: "OrderDynamicPropertyObjectValue",
                newName: "IX_ObjectType_ShipmentId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDynamicProperty_ObjectType_PaymentInId",
                table: "OrderDynamicPropertyObjectValue",
                newName: "IX_ObjectType_PaymentInId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDynamicProperty_ObjectType_ObjectId",
                table: "OrderDynamicPropertyObjectValue",
                newName: "IX_ObjectType_ObjectId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDynamicProperty_ObjectType_LineItemId",
                table: "OrderDynamicPropertyObjectValue",
                newName: "IX_ObjectType_LineItemId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDynamicProperty_ObjectType_CustomerOrderId",
                table: "OrderDynamicPropertyObjectValue",
                newName: "IX_ObjectType_CustomerOrderId");
        }
    }
}
