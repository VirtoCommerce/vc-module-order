using System.Threading.Tasks;
using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.OrdersModule.Data.Model;

namespace VirtoCommerce.OrdersModule.Data.Repositories.Cosmos
{
    public class CosmosOrderDBContext : DbContextWithTriggers
    {
        public CosmosOrderDBContext(DbContextOptions<CosmosOrderDBContext> options)
    : base(options)
        {
        }

        protected CosmosOrderDBContext(DbContextOptions options)
            : base(options)
        {
        }

        public async Task AddIndexes()
        {
            /*
            // Composite indexes required to order by several properties: https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-manage-indexing-policy?tabs=dotnetv3%2Cpythonv3#composite-index
            // Code below based on doc https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-manage-indexing-policy?tabs=dotnetv3%2Cpythonv3#dotnet-sdk
            // This should not be used in real life, because it causes indexes to rebuild on each module init.

            var cosmosClient = Database.GetCosmosClient();
            var container = cosmosClient.GetContainer("OrdersModule", "Orders");

            var containerResponse = await container.ReadContainerAsync();
            containerResponse.Resource.IndexingPolicy.IndexingMode = IndexingMode.Consistent;
            containerResponse.Resource.IndexingPolicy.CompositeIndexes.Clear();
            containerResponse.Resource.IndexingPolicy.CompositeIndexes.Add(new Collection<CompositePath> {
                new CompositePath { Path = "/CreatedDate" },
                new CompositePath { Path = "/Id" }
            });

            containerResponse.Resource.IndexingPolicy.CompositeIndexes.Add(new Collection<CompositePath> {
                new CompositePath { Path = "/CreatedDate", Order = CompositePathSortOrder.Descending },
                new CompositePath { Path = "/Id" }
            });

            await cosmosClient.GetContainer("OrdersModule", "Orders").ReplaceContainerAsync(containerResponse.Resource);

            /*
            await Database.GetCosmosClient().GetDatabase("OrdersModule").DefineContainer(name: "Orders", partitionKeyPath: "/CustomerId")
            .WithIndexingPolicy()
                .WithIncludedPaths()
                    .Path("/*")
                .Attach()
                .WithExcludedPaths()
                    .Path("/\"_etag\"/?")
                .Attach()
                .WithCompositeIndex()
                    .Path("/CreatedDate", CompositePathSortOrder.Ascending)
                    .Path("/Number", CompositePathSortOrder.Ascending)
                .Attach()
            .Attach()
            .CreateIfNotExistsAsync();
            */
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultContainer("Orders");

            modelBuilder.Ignore(typeof(AddressEntity));
            modelBuilder.Ignore(typeof(DiscountEntity));
            modelBuilder.Ignore(typeof(LineItemEntity));
            modelBuilder.Ignore(typeof(OperationEntity));
            modelBuilder.Ignore(typeof(OrderDynamicPropertyObjectValueEntity));
            modelBuilder.Ignore(typeof(PaymentGatewayTransactionEntity));
            modelBuilder.Ignore(typeof(PaymentInEntity));
            modelBuilder.Ignore(typeof(ShipmentEntity));
            modelBuilder.Ignore(typeof(ShipmentItemEntity));
            modelBuilder.Ignore(typeof(ShipmentPackageEntity));
            modelBuilder.Ignore(typeof(TaxDetailEntity));

            modelBuilder.Entity<CustomerOrderEntity>()
                .ToContainer("Orders")
                .HasPartitionKey(o => o.CustomerId)
                .HasKey(o => o.Id)
                ;

            modelBuilder.Entity<CustomerOrderEntity>().OwnsMany(x => x.TaxDetails)
                .Ignore(x => x.Shipment)
                .Ignore(x => x.LineItem)
                .Ignore(x => x.PaymentIn)
                ;
            modelBuilder.Entity<CustomerOrderEntity>().OwnsMany(x => x.Addresses)
                .Ignore(x => x.Shipment)
                .Ignore(x => x.PaymentIn);
            modelBuilder.Entity<CustomerOrderEntity>().OwnsMany(x => x.InPayments)
                .Ignore(x => x.Shipment)
                .Ignore(x => x.Addresses)
                .Ignore(x => x.Discounts)
                .Ignore(x => x.TaxDetails)
                .Ignore(x => x.DynamicPropertyObjectValues)
                .OwnsMany(x => x.Transactions)
                ;
            modelBuilder.Entity<CustomerOrderEntity>().OwnsMany(x => x.Items)
                .Ignore(x => x.Discounts)
                .Ignore(x => x.TaxDetails)
                .Ignore(x => x.DynamicPropertyObjectValues)
                .Ignore(x => x.ShipmentItems)
                ;
            modelBuilder.Entity<CustomerOrderEntity>().OwnsMany(x => x.Shipments)
                .Ignore(x => x.InPayments)
                .Ignore(x => x.Addresses)
                .Ignore(x => x.Discounts)
                .Ignore(x => x.TaxDetails)
                .Ignore(x => x.DynamicPropertyObjectValues)
                .OwnsMany(x => x.Items)
                    .Ignore(x => x.LineItem)
                ;
            modelBuilder.Entity<CustomerOrderEntity>().OwnsMany(x => x.Discounts)
                .Ignore(x => x.Shipment)
                .Ignore(x => x.PaymentIn)
                .Ignore(x => x.LineItem);

            modelBuilder.Entity<CustomerOrderEntity>().OwnsMany(x => x.DynamicPropertyObjectValues)
                .Ignore(x => x.Shipment)
                .Ignore(x => x.PaymentIn)
                .Ignore(x => x.LineItem);
        }
    }
}
