namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LanguageColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerOrder", "LanguageCode", c => c.String(maxLength: 16));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomerOrder", "LanguageCode");
        }
    }
}
