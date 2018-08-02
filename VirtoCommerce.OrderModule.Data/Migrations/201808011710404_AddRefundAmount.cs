namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRefundAmount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderPaymentGatewayTransaction", "RefundAmount", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrderPaymentGatewayTransaction", "RefundAmount");
        }
    }
}
