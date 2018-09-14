namespace VirtoCommerce.OrderModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddedAmountColumnToPaymentInEntity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderPaymentIn", "Amount", c => c.Decimal(nullable: false, storeType: "money"));
            Sql(@"UPDATE OP SET OP.Amount = OO.Sum
				  FROM dbo.OrderPaymentIn AS OP
                  JOIN dbo.OrderOperation AS OO ON OP.Id = OO.Id");
        }

        public override void Down()
        {
            DropColumn("dbo.OrderPaymentIn", "Amount");
        }
    }
}
