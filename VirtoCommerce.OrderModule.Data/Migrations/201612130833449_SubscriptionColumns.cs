namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SubscriptionColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerOrder", "SubscriptionId", c => c.String(maxLength: 64));
            AddColumn("dbo.CustomerOrder", "SubscriptionNumber", c => c.String(maxLength: 64));
            AddColumn("dbo.CustomerOrder", "IsPrototype", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomerOrder", "IsPrototype");
            DropColumn("dbo.CustomerOrder", "SubscriptionNumber");
            DropColumn("dbo.CustomerOrder", "SubscriptionId");
        }
    }
}
