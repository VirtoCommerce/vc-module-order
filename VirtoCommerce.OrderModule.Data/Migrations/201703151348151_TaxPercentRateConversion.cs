namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaxPercentRateConversion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerOrder", "TaxPercentRate", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.OrderLineItem ", "TaxPercentRate", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.OrderPaymentIn", "TaxPercentRate", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.OrderShipment", "TaxPercentRate", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomerOrder", "TaxPercentRate");
        }
    }
}
