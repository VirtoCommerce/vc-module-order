using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;
using Bogus;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Tests
{
    [Trait("Category", "CI")]
    public class OrderInventoryAdjustmentTests
    {
        private const string TestStoreId = "TestStore";
        private const string TestFulfillmentCenterId = "TestFulfillmentCenter";

        private static readonly CustomerOrder InitialOrder = new CustomerOrder
        {
            StoreId = TestStoreId,
            Items = new List<LineItem>
            {
                new LineItem
                {
                    FulfillmentCenterId = TestFulfillmentCenterId,
                    Id = "{01234567-89ab-cdef-0123-456789abcdef}",
                    ProductId = "shoes",
                    Quantity = 2
                },
                new LineItem
                {
                    FulfillmentCenterId = TestFulfillmentCenterId,
                    Id = "{abcdef01-2345-6789-abcd-ef0123456789}",
                    ProductId = "t-shirt",
                    Quantity = 2
                }
            }
        };

        private static readonly CustomerOrder ChangedOrder = new CustomerOrder
        {
            StoreId = TestStoreId,
            Items = new List<LineItem>
            {
                new LineItem
                {
                    FulfillmentCenterId = TestFulfillmentCenterId,
                    Id = "{01234567-89ab-cdef-0123-456789abcdef}",
                    ProductId = "shoes",
                    Quantity = 5
                },
                new LineItem
                {
                    FulfillmentCenterId = TestFulfillmentCenterId,
                    Id = "{fedcba98-7654-3210-0123-456789abcdef}",
                    ProductId = "t-shirt-new",
                    Quantity = 1
                }
            }
        };

        public static readonly IList<object[]> TestData = new List<object[]>
        {
            new object[]
            {
                new GenericChangedEntry<CustomerOrder>(ChangedOrder, InitialOrder, EntryState.Modified),
                new[]
                {
                    new ProductInventoryChange
                    {
                        FulfillmentCenterId = TestFulfillmentCenterId,
                        ProductId = "shoes",
                        QuantityDelta = 3
                    },
                    new ProductInventoryChange
                    {
                        FulfillmentCenterId = TestFulfillmentCenterId,
                        ProductId = "t-shirt-new",
                        QuantityDelta = 1
                    },
                    new ProductInventoryChange
                    {
                        FulfillmentCenterId = TestFulfillmentCenterId,
                        ProductId = "t-shirt",
                        QuantityDelta = -2
                    }
                }
            },
            new object[]
            {
                new GenericChangedEntry<CustomerOrder>(InitialOrder, InitialOrder, EntryState.Added),
                new[]
                {
                    new ProductInventoryChange
                    {
                        FulfillmentCenterId = TestFulfillmentCenterId,
                        ProductId = "shoes",
                        QuantityDelta = 2
                    },
                    new ProductInventoryChange
                    {
                        FulfillmentCenterId = TestFulfillmentCenterId,
                        ProductId = "t-shirt",
                        QuantityDelta = 2
                    }
                }
            },
            new object[]
            {
                new GenericChangedEntry<CustomerOrder>(InitialOrder, InitialOrder, EntryState.Deleted),
                new[]
                {
                    new ProductInventoryChange
                    {
                        FulfillmentCenterId = TestFulfillmentCenterId,
                        ProductId = "shoes",
                        QuantityDelta = -2
                    },
                    new ProductInventoryChange
                    {
                        FulfillmentCenterId = TestFulfillmentCenterId,
                        ProductId = "t-shirt",
                        QuantityDelta = -2
                    }
                }
            }
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task AdjustInventoryHandler_GetProductInventoryChanges_ForOrderChanges(GenericChangedEntry<CustomerOrder> orderChangedEntry,
            IEnumerable<ProductInventoryChange> expectedChanges)
        {
            // Arrange
            var inventoryServiceMock = new Mock<IInventoryService>();
            var settingsManagerMock = new Mock<ISettingsManager>();
            var itemServiceMock = new Mock<IItemService>();

            var storeServiceMock = new Mock<IStoreService>();
            storeServiceMock.Setup(x => x.GetByIdAsync(TestStoreId, null))
                .ReturnsAsync(new Store { MainFulfillmentCenterId = TestFulfillmentCenterId });

            var targetHandler = new AdjustInventoryOrderChangedEventHandler(inventoryServiceMock.Object,
                storeServiceMock.Object, settingsManagerMock.Object, itemServiceMock.Object);

            // Act
            var actualChanges = await targetHandler.GetProductInventoryChangesFor(orderChangedEntry);

            // Assert
            var equalityComparer = AnonymousComparer.Create((ProductInventoryChange x) => $"{x.FulfillmentCenterId} {x.ProductId} {x.QuantityDelta}");
            Assert.Equal(expectedChanges, actualChanges, equalityComparer);
        }

        [Theory]
        [InlineData(10, 5, 5)]
        [InlineData(100, 99, 1)]
        [InlineData(10, 11, 0)]
        [InlineData(0, 5, 0)]        
        public async Task TryAdjustOrderInventory_TrackInventoryTrue(long inStockQty, int quantityDelta, long resultInStockQty)
        {
            // Arrange

            var testPproducts = new Faker<CatalogProduct>()
                .RuleFor(i => i.Id, f => f.Random.Guid().ToString())
                .RuleFor(i => i.TrackInventory, f => true);

            var product = testPproducts.Generate();

            var inventoryInfo = new InventoryInfo()
            {
                ProductId = product.Id,
                FulfillmentCenterId = TestFulfillmentCenterId,
                InStockQuantity = inStockQty
            };

            var productInventoryChange = new ProductInventoryChange()
            {
                ProductId = product.Id,
                FulfillmentCenterId = TestFulfillmentCenterId,
                QuantityDelta = quantityDelta
            };


            var inventoryServiceMock = new Mock<IInventoryService>();
            inventoryServiceMock.Setup(x => x.GetProductsInventoryInfosAsync(new[] { product.Id }, null))
                .ReturnsAsync(new[] { inventoryInfo });
                
            var settingsManagerMock = new Mock<ISettingsManager>();
            var itemServiceMock = new Mock<IItemService>();
            itemServiceMock.Setup(x => x.GetByIdsAsync(new[] { product.Id }, ItemResponseGroup.None.ToString(), null))
                .ReturnsAsync(new[] { product });

            var storeServiceMock = new Mock<IStoreService>();
            storeServiceMock.Setup(x => x.GetByIdAsync(TestStoreId, null))
                .ReturnsAsync(new Store { MainFulfillmentCenterId = TestFulfillmentCenterId });

            var targetHandler = new AdjustInventoryOrderChangedEventHandler(inventoryServiceMock.Object,
                storeServiceMock.Object, settingsManagerMock.Object, itemServiceMock.Object);

            // Act
            await targetHandler.TryAdjustOrderInventory(new[] { productInventoryChange });

            // Assert           
            Assert.Equal(resultInStockQty, inventoryInfo.InStockQuantity);
        }

        [Theory]
        [InlineData(10, 5, 10)]
        [InlineData(100, 99, 100)]
        [InlineData(10, 11, 10)]
        [InlineData(0, 5, 0)]
        public async Task TryAdjustOrderInventory_TrackInventoryFalse(long inStockQty, int quantityDelta, long resultInStockQty)
        {
            // Arrange

            var testPproducts = new Faker<CatalogProduct>()
                .RuleFor(i => i.Id, f => f.Random.Guid().ToString())
                .RuleFor(i => i.TrackInventory, f => false);

            var product = testPproducts.Generate();

            var inventoryInfo = new InventoryInfo()
            {
                ProductId = product.Id,
                FulfillmentCenterId = TestFulfillmentCenterId,
                InStockQuantity = inStockQty
            };

            var productInventoryChange = new ProductInventoryChange()
            {
                ProductId = product.Id,
                FulfillmentCenterId = TestFulfillmentCenterId,
                QuantityDelta = quantityDelta
            };


            var inventoryServiceMock = new Mock<IInventoryService>();
            inventoryServiceMock.Setup(x => x.GetProductsInventoryInfosAsync(new[] { product.Id }, null))
                .ReturnsAsync(new[] { inventoryInfo });

            var settingsManagerMock = new Mock<ISettingsManager>();
            var itemServiceMock = new Mock<IItemService>();
            itemServiceMock.Setup(x => x.GetByIdsAsync(new[] { product.Id }, ItemResponseGroup.None.ToString(), null))
                .ReturnsAsync(new[] { product });

            var storeServiceMock = new Mock<IStoreService>();
            storeServiceMock.Setup(x => x.GetByIdAsync(TestStoreId, null))
                .ReturnsAsync(new Store { MainFulfillmentCenterId = TestFulfillmentCenterId });

            var targetHandler = new AdjustInventoryOrderChangedEventHandler(inventoryServiceMock.Object,
                storeServiceMock.Object, settingsManagerMock.Object, itemServiceMock.Object);

            // Act
            await targetHandler.TryAdjustOrderInventory(new[] { productInventoryChange });

            // Assert           
            Assert.Equal(resultInStockQty, inventoryInfo.InStockQuantity);
        }

    }
}
