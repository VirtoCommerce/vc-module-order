namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddWorkflowEntity : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Workflow",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Workflow = c.String(nullable: false),
                        MemberId = c.String(maxLength: 128),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        CreatedBy = c.String(maxLength: 64),
                        ModifiedBy = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.CustomerOrder", "WorkflowId", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomerOrder", "WorkflowId");
            DropTable("dbo.Workflow");
        }
    }
}
