using System.Collections.ObjectModel;
using System.Threading.Tasks;
using EntityFrameworkCore.Triggers;
using Microsoft.Azure.Cosmos;
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

            modelBuilder.Entity<CustomerOrderEntity>()
                .ToContainer("Orders")
                .HasPartitionKey(o => o.CustomerId)
                .HasKey(o => o.Id);

            modelBuilder.Entity<TaxDetailEntity>()
                .ToContainer("OrdersDetails")
                .HasPartitionKey(o => o.CustomerOrderId)
                .HasKey(o => o.Id);

            modelBuilder.Entity<AddressEntity>()
                .ToContainer("OrdersDetails")
                .HasPartitionKey(o => o.CustomerOrderId)
                .HasKey(o => o.Id);

            modelBuilder.Entity<ShipmentEntity>()
                .ToContainer("OrdersDetails")
                .HasPartitionKey(o => o.CustomerOrderId)
                .HasKey(o => o.Id);
            modelBuilder.Entity<ShipmentEntity>().OwnsMany(x => x.Items);

            modelBuilder.Ignore<ShipmentPackageEntity>();

            modelBuilder.Entity<PaymentInEntity>()
                .ToContainer("OrdersDetails")
                .HasPartitionKey(o => o.CustomerOrderId)
                .HasKey(o => o.Id);

            modelBuilder.Entity<PaymentInEntity>().OwnsMany(x => x.Transactions);

            modelBuilder.Entity<LineItemEntity>()
                .ToContainer("OrdersDetails")
                .HasPartitionKey(o => o.CustomerOrderId)
                .HasKey(o => o.Id);
            modelBuilder.Entity<LineItemEntity>().OwnsMany(x => x.ShipmentItems);

            modelBuilder.Entity<DiscountEntity>()
                .ToContainer("OrdersDetails")
                .HasPartitionKey(o => o.CustomerOrderId)
                .HasKey(o => o.Id);

            modelBuilder.Entity<OrderDynamicPropertyObjectValueEntity>()
                .ToContainer("OrdersDetails")
                .HasPartitionKey(o => o.CustomerOrderId)
                .HasKey(o => o.Id);

        }
    }
}
