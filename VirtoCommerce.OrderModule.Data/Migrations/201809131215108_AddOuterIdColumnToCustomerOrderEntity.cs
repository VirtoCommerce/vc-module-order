namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOuterIdColumnToCustomerOrderEntity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerOrder", "OuterId", c => c.String(maxLength: 64));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomerOrder", "OuterId");
        }
    }
}
