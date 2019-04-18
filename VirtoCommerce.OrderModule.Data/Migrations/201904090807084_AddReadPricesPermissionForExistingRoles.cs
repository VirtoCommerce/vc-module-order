namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddReadPricesPermissionForExistingRoles : DbMigration
    {
        public override void Up()
        {
            //The "order:read_prices" permission must be added to all roles which contains the "order:read" permission to existing users could see the prices
            Sql(@"IF (EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='PlatformPermission') AND 
                      EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='PlatformRolePermission'))
                  BEGIN
	                  IF NOT EXISTS(SELECT id FROM [dbo].[PlatformPermission] WHERE id = 'order:read_prices')
                      BEGIN
                          INSERT INTO [dbo].[PlatformPermission]([Id],[Name],[Description],[CreatedDate],[ModifiedDate],[CreatedBy],[ModifiedBy])
                          VALUES ('order:read_prices', 'View order prices', 'Permission to view order prices.', getdate(), getdate(), 'unknown', 'unknown');
                      END

	                  INSERT INTO [dbo].[PlatformRolePermission]([Id],[RoleId],[PermissionId],[CreatedDate],[ModifiedDate],[CreatedBy],[ModifiedBy])
                      SELECT NEWID(), b.RoleId, 'order:read_prices', getdate(), getdate(), 'unknown', 'unknown'
                      FROM [dbo].[PlatformRolePermission] b
                      WHERE b.PermissionId = 'order:read';
                  END");
        }
    }
}
