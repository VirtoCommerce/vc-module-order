namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateOrderWorkflow : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrderWorkflow",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        OrderId = c.String(nullable: false, maxLength: 128),
                        WorkflowId = c.String(nullable: false, maxLength: 128),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        CreatedBy = c.String(maxLength: 64),
                        ModifiedBy = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.OrderWorkflow");
        }
    }
}
