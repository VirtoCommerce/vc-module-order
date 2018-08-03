namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddressName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderAddress", "Name", c => c.String(maxLength: 2048));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrderAddress", "Name");
        }
    }
}
