namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUnusedPrices : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.OrderShipment", "BasePrice");
            DropColumn("dbo.OrderShipment", "BasePriceWithTax");
            DropColumn("dbo.OrderLineItem", "BasePrice");
            DropColumn("dbo.OrderLineItem", "BasePriceWithTax");
        }
        
        public override void Down()
        {
            AddColumn("dbo.OrderLineItem", "BasePriceWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderLineItem", "BasePrice", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderShipment", "BasePriceWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderShipment", "BasePrice", c => c.Decimal(nullable: false, storeType: "money"));
        }
    }
}
