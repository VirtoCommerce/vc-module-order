namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LongerEmailAddress : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OrderAddress", "Email", c => c.String(maxLength: 254));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.OrderAddress", "Email", c => c.String(maxLength: 64));
        }
    }
}
