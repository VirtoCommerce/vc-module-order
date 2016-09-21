namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TotalsAndTaxesChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerOrder", "DiscountAmount", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.CustomerOrder", "TaxTotal", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.CustomerOrder", "Total", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.CustomerOrder", "SubTotal", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.CustomerOrder", "SubTotalWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.CustomerOrder", "ShippingTotal", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.CustomerOrder", "ShippingTotalWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.CustomerOrder", "HandlingTotal", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.CustomerOrder", "HandlingTotalWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.CustomerOrder", "DiscountTotal", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.CustomerOrder", "DiscountTotalWithTax", c => c.Decimal(nullable: false, storeType: "money"));

            AddColumn("dbo.OrderShipment", "Price", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderShipment", "PriceWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderShipment", "DiscountAmountWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderShipment", "Total", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderShipment", "TotalWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderShipment", "TaxTotal", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderShipment", "TaxPercentRate", c => c.Decimal(nullable: false, precision: 3, scale: 3));

            AddColumn("dbo.OrderDiscount", "DiscountAmountWithTax", c => c.Decimal(nullable: false, storeType: "money"));

            AddColumn("dbo.OrderLineItem", "PriceWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderLineItem", "DiscountAmountWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderLineItem", "TaxTotal", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderLineItem", "TaxPercentRate", c => c.Decimal(nullable: false, precision: 3, scale: 3));

            Sql("UPDATE dbo.CustomerOrder SET DiscountAmount = D.DiscountAmount FROM dbo.OrderDiscount as D WHERE D.CustomerOrderId = dbo.CustomerOrder.Id");
            Sql("UPDATE dbo.CustomerOrder SET Total = O.Sum + O.Tax, TaxTotal = O.Tax FROM dbo.OrderOperation as O WHERE O.Id = dbo.CustomerOrder.Id");

            Sql("UPDATE dbo.OrderShipment SET DiscountAmount = D.DiscountAmount FROM dbo.OrderDiscount as D WHERE D.ShipmentId = dbo.OrderShipment.Id");
            Sql("UPDATE dbo.OrderShipment SET Total = O.Sum, TaxTotal = O.Tax, Price = O.Sum - DiscountAmount FROM dbo.OrderOperation as O WHERE O.Id = dbo.OrderShipment.Id");
            Sql("UPDATE dbo.OrderShipment SET TaxPercentRate = TaxTotal / Total WHERE Total > 0");

            Sql("UPDATE dbo.OrderLineItem SET DiscountAmount = D.DiscountAmount FROM dbo.OrderDiscount as D WHERE D.LineItemId = dbo.OrderLineItem.Id");
            Sql("UPDATE dbo.OrderLineItem SET TaxTotal = Tax");
            Sql("UPDATE dbo.OrderLineItem SET TaxPercentRate = TaxTotal / (Price * Quantity - DiscountAmount) WHERE Price > 0 AND Quantity > 0");
                     

            DropColumn("dbo.OrderOperation", "TaxIncluded");          
            DropColumn("dbo.OrderOperation", "Tax");
            DropColumn("dbo.OrderLineItem", "BasePrice");
            DropColumn("dbo.OrderLineItem", "Tax");
        }

        public override void Down()
        {
            AddColumn("dbo.OrderLineItem", "Tax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderLineItem", "BasePrice", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderOperation", "Tax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderOperation", "TaxIncluded", c => c.Boolean(nullable: false));
            DropColumn("dbo.OrderLineItem", "TaxPercentRate");
            DropColumn("dbo.OrderLineItem", "TaxTotal");
            DropColumn("dbo.OrderLineItem", "DiscountAmountWithTax");
            DropColumn("dbo.OrderLineItem", "PriceWithTax");
            DropColumn("dbo.OrderDiscount", "DiscountAmountWithTax");
            DropColumn("dbo.OrderShipment", "TaxPercentRate");
            DropColumn("dbo.OrderShipment", "TaxTotal");
            DropColumn("dbo.OrderShipment", "TotalWithTax");
            DropColumn("dbo.OrderShipment", "Total");
            DropColumn("dbo.OrderShipment", "DiscountAmountWithTax");
            DropColumn("dbo.OrderShipment", "PriceWithTax");
            DropColumn("dbo.OrderShipment", "Price");
            DropColumn("dbo.CustomerOrder", "DiscountTotalWithTax");
            DropColumn("dbo.CustomerOrder", "DiscountTotal");
            DropColumn("dbo.CustomerOrder", "HandlingTotalWithTax");
            DropColumn("dbo.CustomerOrder", "HandlingTotal");
            DropColumn("dbo.CustomerOrder", "ShippingTotalWithTax");
            DropColumn("dbo.CustomerOrder", "ShippingTotal");
            DropColumn("dbo.CustomerOrder", "SubTotalWithTax");
            DropColumn("dbo.CustomerOrder", "SubTotal");
            DropColumn("dbo.CustomerOrder", "Total");
            DropColumn("dbo.CustomerOrder", "TaxTotal");
            DropColumn("dbo.CustomerOrder", "DiscountAmount");
        }
    }
}
