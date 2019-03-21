namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateWorkflow : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Workflow",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        OrganizationId = c.String(nullable: false, maxLength: 128),
                        WorkflowName = c.String(nullable: false, maxLength: 128),
                        JsonPath = c.String(nullable: false, maxLength: 500),
                        Status = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        CreatedBy = c.String(maxLength: 64),
                        ModifiedBy = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Workflow");
        }
    }
}
