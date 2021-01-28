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
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model;
using System;
using System.Linq;
using FluentAssertions;

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
            var actualChanges = targetHandler.GetProductInventoryChangesFor(orderChangedEntry);

            // Assert
            var equalityComparer = AnonymousComparer.Create((ProductInventoryChange x) => $"{x.FulfillmentCenterId} {x.ProductId} {x.QuantityDelta}");
            Assert.Equal(expectedChanges, actualChanges, equalityComparer);
        }

        [Theory]
        [InlineData(10, 5, 5, true)]
        [InlineData(100, 99, 1, true)]
        [InlineData(10, 11, 0, true)]
        [InlineData(0, 5, 0, true)]
        [InlineData(10, 5, 10, false)]
        [InlineData(100, 99, 100, false)]
        [InlineData(10, 11, 10, false)]
        [InlineData(0, 5, 0, false)]
        public async Task TryAdjustOrderInventory_TrackInventory_InStockQtyShouldBeEqualExpectedInStockQty(long inStockQty, int quantityDelta, long expectedInStockQty, bool trackInventory)
        {
            // Arrange
            var productId = Guid.NewGuid().ToString();
            var responseGroup = ItemResponseGroup.None.ToString();
            var inventoryServiceMock = new Mock<IInventoryService>();
            var itemServiceMock = new Mock<IItemService>();

            var product = Mock.Of<CatalogProduct>(t => t.Id == productId && t.TrackInventory == trackInventory);
            var inventoryInfo = Mock.Of<InventoryInfo>(
                x => x.ProductId == productId && x.FulfillmentCenterId == TestFulfillmentCenterId
                                              && x.InStockQuantity == inStockQty);
            var productInventoryChange = Mock.Of<ProductInventoryChange>(
                x => x.ProductId == productId && x.FulfillmentCenterId == TestFulfillmentCenterId
                                              && x.QuantityDelta == quantityDelta);

            inventoryServiceMock.Setup(x => x.GetProductsInventoryInfosAsync(new[] { productId }, null))
                .ReturnsAsync(new[] { inventoryInfo });

            itemServiceMock.Setup(x => x.GetByIdsAsync(new[] { productId }, responseGroup, null))
                .ReturnsAsync(new[] { product });

            var handler = new AdjustInventoryOrderChangedEventHandler(inventoryServiceMock.Object,
                Mock.Of<IStoreService>(), Mock.Of<ISettingsManager>(), itemServiceMock.Object);

            // Act
            await handler.TryAdjustOrderInventory(new[] { productInventoryChange });

            // Assert
            inventoryServiceMock.VerifyAll();
            itemServiceMock.VerifyAll();
            Assert.Equal(expectedInStockQty, inventoryInfo.InStockQuantity);
        }

        [Fact]
        public async Task GetFullfilmentCenterForLineItemAsync_GetAdditionalFfc_FfcFound()
        {
            // Arrange
            var mainFulfillmentCenterId = "MainFulfillmentCenterId";
            var additionalFulfillmentCenterId = "additionalFulfillmentCenterId";
            var changedOrder = new CustomerOrder
            {
                StoreId = TestStoreId,
                Items = new List<LineItem> { new LineItem  { Id = "{01234567-89ab-cdef-0123-456789abcdef}",  ProductId = "shoes", Quantity = 1 }}
            };
            var changedEntry = new GenericChangedEntry<CustomerOrder>(changedOrder, changedOrder, EntryState.Added);

            var inventoryServiceMock = new Mock<IInventoryService>();
            var settingsManagerMock = new Mock<ISettingsManager>();
            var itemServiceMock = new Mock<IItemService>();

            var storeServiceMock = new Mock<IStoreService>();
            storeServiceMock.Setup(x => x.GetByIdAsync(TestStoreId, It.IsAny<string>()))
                .ReturnsAsync(new Store { MainFulfillmentCenterId = mainFulfillmentCenterId, AdditionalFulfillmentCenterIds = new List<string>() {additionalFulfillmentCenterId}});

            inventoryServiceMock.Setup(x=>x.GetProductsInventoryInfosAsync(It.IsAny<IEnumerable<string>>(), null))
                .ReturnsAsync( new List<InventoryInfo>()
                {
                    new InventoryInfo() { InStockQuantity = 0, FulfillmentCenterId = mainFulfillmentCenterId},
                    new InventoryInfo() { InStockQuantity = 10, FulfillmentCenterId = additionalFulfillmentCenterId}
                });

            var targetHandler = new AdjustInventoryOrderChangedEventHandler(inventoryServiceMock.Object,
                storeServiceMock.Object, settingsManagerMock.Object, itemServiceMock.Object);

            // Act
            var actualChanges = targetHandler.GetProductInventoryChangesFor(changedEntry);

            // Assert
            actualChanges.Should().HaveCount(1);
            actualChanges.First().FulfillmentCenterId.Should().Be(additionalFulfillmentCenterId);

        }


    }
}
