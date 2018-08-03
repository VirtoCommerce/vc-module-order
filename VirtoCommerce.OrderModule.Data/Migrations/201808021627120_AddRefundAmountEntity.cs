namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRefundAmountEntity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderPaymentGatewayTransaction", "RefundAmount", c => c.Decimal(nullable: false, storeType: "money"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrderPaymentGatewayTransaction", "RefundAmount");
        }
    }
}
