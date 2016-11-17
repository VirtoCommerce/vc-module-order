namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PaymentInSelfPricesFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerOrder", "PaymentTotal", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.CustomerOrder", "PaymentTotalWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderPaymentIn", "TaxType", c => c.String(maxLength: 64));
            AddColumn("dbo.OrderPaymentIn", "Price", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderPaymentIn", "PriceWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderPaymentIn", "DiscountAmount", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderPaymentIn", "DiscountAmountWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderPaymentIn", "Total", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderPaymentIn", "TotalWithTax", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderPaymentIn", "TaxTotal", c => c.Decimal(nullable: false, storeType: "money"));
            AddColumn("dbo.OrderPaymentIn", "TaxPercentRate", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.OrderDiscount", "PaymentInId", c => c.String(maxLength: 128));
            AddColumn("dbo.OrderTaxDetail", "PaymentInId", c => c.String(maxLength: 128));
            CreateIndex("dbo.OrderDiscount", "PaymentInId");
            CreateIndex("dbo.OrderTaxDetail", "PaymentInId");
            AddForeignKey("dbo.OrderTaxDetail", "PaymentInId", "dbo.OrderPaymentIn", "Id");
            AddForeignKey("dbo.OrderDiscount", "PaymentInId", "dbo.OrderPaymentIn", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderDiscount", "PaymentInId", "dbo.OrderPaymentIn");
            DropForeignKey("dbo.OrderTaxDetail", "PaymentInId", "dbo.OrderPaymentIn");
            DropIndex("dbo.OrderTaxDetail", new[] { "PaymentInId" });
            DropIndex("dbo.OrderDiscount", new[] { "PaymentInId" });
            DropColumn("dbo.OrderTaxDetail", "PaymentInId");
            DropColumn("dbo.OrderDiscount", "PaymentInId");
            DropColumn("dbo.OrderPaymentIn", "TaxPercentRate");
            DropColumn("dbo.OrderPaymentIn", "TaxTotal");
            DropColumn("dbo.OrderPaymentIn", "TotalWithTax");
            DropColumn("dbo.OrderPaymentIn", "Total");
            DropColumn("dbo.OrderPaymentIn", "DiscountAmountWithTax");
            DropColumn("dbo.OrderPaymentIn", "DiscountAmount");
            DropColumn("dbo.OrderPaymentIn", "PriceWithTax");
            DropColumn("dbo.OrderPaymentIn", "Price");
            DropColumn("dbo.OrderPaymentIn", "TaxType");
            DropColumn("dbo.CustomerOrder", "PaymentTotalWithTax");
            DropColumn("dbo.CustomerOrder", "PaymentTotal");
        }
    }
}
