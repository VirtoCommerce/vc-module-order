using System.Data.Entity.Migrations;

namespace VirtoCommerce.OrderModule.Data.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<Repositories.OrderRepositoryImpl>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Repositories.OrderRepositoryImpl context)
        {
        }
    }
}
