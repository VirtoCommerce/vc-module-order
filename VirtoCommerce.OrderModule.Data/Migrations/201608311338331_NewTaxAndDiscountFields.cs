namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewTaxAndDiscountFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerOrder", "DiscountAmount", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.CustomerOrder", "DiscountAmountWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderShipment", "BasePrice", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderShipment", "BasePriceWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderShipment", "Price", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderShipment", "PriceWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderShipment", "DiscountAmountWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderDiscount", "DiscountAmountWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderLineItem", "BasePriceWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderLineItem", "PriceWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderLineItem", "DiscountAmountWithTax", c => c.Decimal(nullable: false, storeType: "money"));

            Sql("UPDATE dbo.OrderDiscount SET DiscountAmountWithTax = DiscountAmount");

            Sql("UPDATE dbo.OrderShipment SET BasePrice = O.Sum, BasePriceWithTax = O.Tax + O.Sum, Price = O.Sum, PriceWithTax = O.Tax + O.Sum FROM dbo.OrderOperation as O WHERE O.Id = dbo.OrderShipment.Id");
            Sql("UPDATE dbo.OrderShipment SET DiscountAmount = D.DiscountAmount, DiscountAmountWithTax = D.DiscountAmountWithTax FROM dbo.OrderDiscount as D WHERE D.ShipmentId = dbo.OrderShipment.Id");

            Sql("UPDATE dbo.OrderLineItem SET BasePriceWithTax = BasePrice + Tax / Quantity, PriceWithTax = Price +  Tax / Quantity");
            Sql("UPDATE dbo.OrderLineItem SET DiscountAmount = D.DiscountAmount, DiscountAmountWithTax = D.DiscountAmountWithTax FROM dbo.OrderDiscount as D WHERE D.LineItemId = dbo.OrderLineItem.Id");

            Sql("UPDATE dbo.CustomerOrder SET DiscountAmount = D.DiscountAmount, DiscountAmountWithTax = D.DiscountAmountWithTax FROM dbo.OrderDiscount as D WHERE D.CustomerOrderId = dbo.CustomerOrder.Id");
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrderLineItem", "DiscountAmountWithTax");
            DropColumn("dbo.OrderLineItem", "PriceWithTax");
            DropColumn("dbo.OrderLineItem", "BasePriceWithTax");
            DropColumn("dbo.OrderDiscount", "DiscountAmountWithTax");
            DropColumn("dbo.OrderShipment", "DiscountAmountWithTax");
            DropColumn("dbo.OrderShipment", "PriceWithTax");
            DropColumn("dbo.OrderShipment", "Price");
            DropColumn("dbo.OrderShipment", "BasePriceWithTax");
            DropColumn("dbo.OrderShipment", "BasePrice");
            DropColumn("dbo.CustomerOrder", "DiscountAmountWithTax");
            DropColumn("dbo.CustomerOrder", "DiscountAmount");
        }
    }
}
