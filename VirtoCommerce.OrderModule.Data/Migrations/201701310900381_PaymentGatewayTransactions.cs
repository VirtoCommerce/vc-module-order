namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PaymentGatewayTransactions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrderPaymentGatewayTransaction",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Amount = c.Decimal(nullable: false, storeType: "money"),
                        Currency = c.String(maxLength: 3),
                        IsProcessed = c.Boolean(nullable: false),
                        ProcessedDate = c.DateTime(),
                        ProcessError = c.String(maxLength: 2048),
                        ProcessAttemptCount = c.Int(nullable: false),
                        RequestData = c.String(),
                        ResponseData = c.String(),
                        ResponseCode = c.String(maxLength: 64),
                        GatewayIpAddress = c.String(maxLength: 128),
                        Type = c.String(maxLength: 64),
                        Status = c.String(maxLength: 64),
                        Note = c.String(maxLength: 2048),
                        PaymentInId = c.String(nullable: false, maxLength: 128),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        CreatedBy = c.String(maxLength: 64),
                        ModifiedBy = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OrderPaymentIn", t => t.PaymentInId, cascadeDelete: true)
                .Index(t => t.PaymentInId);
            
            AddColumn("dbo.CustomerOrder", "ShoppingCartId", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderPaymentGatewayTransaction", "PaymentInId", "dbo.OrderPaymentIn");
            DropIndex("dbo.OrderPaymentGatewayTransaction", new[] { "PaymentInId" });
            DropColumn("dbo.CustomerOrder", "ShoppingCartId");
            DropTable("dbo.OrderPaymentGatewayTransaction");
        }
    }
}
