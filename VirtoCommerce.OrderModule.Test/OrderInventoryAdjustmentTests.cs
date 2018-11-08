using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.OrderModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.OrderModule.Test
{
    public class OrderInventoryAdjustmentTests
    {
        private class InventoryInfoEqualityComparer : IEqualityComparer<InventoryInfo>
        {
            public bool Equals(InventoryInfo x, InventoryInfo y)
            {
                if (Object.ReferenceEquals(x, y))
                    return true;

                if (x == null || y == null)
                    return false;

                return x.ProductId == y.ProductId &&
                       x.InStockQuantity == y.InStockQuantity;
            }

            public int GetHashCode(InventoryInfo obj)
            {
                return obj.GetHashCode();
            }
        }

        private readonly Mock<IInventoryService> _inventoryServiceMock;
        private readonly Mock<IStoreService> _storeServiceMock;
        private readonly Mock<ISettingsManager> _settingsManagerMock;
        private readonly Mock<IBackgroundJobClient> _backgroundJobClientMock;
        private readonly AdjustInventoryOrderChangedEventHandler _targetHandler;

        public OrderInventoryAdjustmentTests()
        {
            _inventoryServiceMock = new Mock<IInventoryService>();

            _storeServiceMock = new Mock<IStoreService>();

            _settingsManagerMock = new Mock<ISettingsManager>();
            _settingsManagerMock.Setup(x => x.GetValue("Order.AdjustInventory", It.IsAny<bool>()))
                .Returns(true);

            _backgroundJobClientMock = new Mock<IBackgroundJobClient>();

            _targetHandler = new AdjustInventoryOrderChangedEventHandler(_inventoryServiceMock.Object,
                _storeServiceMock.Object, _settingsManagerMock.Object, _backgroundJobClientMock.Object);

            // Setting up Hangfire background job client to immediately execute any queued task
            _backgroundJobClientMock.Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()))
                .Callback((Job job, IState state) => job.Method.Invoke(_targetHandler, job.Args.ToArray()));
        }

        [Fact]
        public async Task AdjustInventoryHandler_AdjustsInventory_OnLineItemQuantityChange()
        {
            // Arrange
            var oldOrder = CreateTestOrder("TESTORDER-000001");

            var newOrder = CreateTestOrder("TESTORDER-000001");
            var newOrderItems = newOrder.Items.ToList();

            // Adjusting quantity for the first item. This should cause taking 3 pieces of "shoes" from the inventory.
            newOrderItems[0].Quantity += 3;

            // Changing Id and ProductId for the second item and adjusting its quantity. This will be treated as removal
            // of "t-shirt" line item and addition of "t-shirt-new" line item, so it should cause the following changes:
            // - taking 1 piece of "t-shirt-new" from the inventory;
            // - returning 2 pieces of "t-shirt" to the inventory.
            newOrderItems[1].Id = "{fedcba98-7654-3210-0123-456789abcdef}";
            newOrderItems[1].ProductId = "t-shirt-new";
            newOrderItems[1].Quantity = 1;

            var changedEntries = new[] { new GenericChangedEntry<CustomerOrder>(newOrder, oldOrder, EntryState.Modified) };
            var orderChangedEvent = new OrderChangedEvent(changedEntries);

            var sourceInventoryInfos = new[]
            {
                new InventoryInfo
                {
                    ProductId = "shoes",
                    InStockQuantity = 5
                },
                new InventoryInfo
                {
                    ProductId = "t-shirt",
                    InStockQuantity = 1
                },
                new InventoryInfo
                {
                    ProductId = "t-shirt-new",
                    InStockQuantity = 2
                }
            };
            _inventoryServiceMock.Setup(x => x.GetProductsInventoryInfos(It.IsAny<IEnumerable<string>>()))
                .Returns(sourceInventoryInfos);

            var expectedInventoryInfos = new[]
            {
                new InventoryInfo
                {
                    ProductId = "shoes",
                    InStockQuantity = 2 // 3 pieces taken
                },
                new InventoryInfo
                {
                    ProductId = "t-shirt-new",
                    InStockQuantity = 1 // 1 piece taken
                },
                new InventoryInfo
                {
                    ProductId = "t-shirt",
                    InStockQuantity = 3 // 2 pieces returned
                }
            };

            IEnumerable<InventoryInfo> actualInventoryInfos = null;
            _inventoryServiceMock.Setup(x => x.UpsertInventories(It.IsAny<IEnumerable<InventoryInfo>>()))
                .Callback((IEnumerable<InventoryInfo> inventoryInfos) => { actualInventoryInfos = inventoryInfos; });

            // Act
            await _targetHandler.Handle(orderChangedEvent);

            // Assert
            Assert.Equal(expectedInventoryInfos, actualInventoryInfos, new InventoryInfoEqualityComparer());
        }

        [Fact]
        public async Task AdjustInventoryHandler_AdjustsInventory_OnOrderCreation()
        {
            // Arrange
            var newOrder = CreateTestOrder("TESTORDER-000001");
            var changedEntries = new[] { new GenericChangedEntry<CustomerOrder>(newOrder, newOrder, EntryState.Added) };
            var orderChangedEvent = new OrderChangedEvent(changedEntries);

            var sourceInventoryInfos = new[]
            {
                new InventoryInfo
                {
                    ProductId = "shoes",
                    InStockQuantity = 5
                },
                new InventoryInfo
                {
                    ProductId = "t-shirt",
                    InStockQuantity = 1
                },
            };
            _inventoryServiceMock.Setup(x => x.GetProductsInventoryInfos(It.IsAny<IEnumerable<string>>()))
                .Returns(sourceInventoryInfos);

            var expectedInventoryInfos = new[]
            {
                new InventoryInfo
                {
                    ProductId = "shoes",
                    InStockQuantity = 3 // 2 pieces taken
                },
                new InventoryInfo
                {
                    ProductId = "t-shirt",
                    InStockQuantity = 0 // 2 pieces taken, InStockQuantity is "stuck" at 0 instead of reaching -1
                }
            };

            IEnumerable<InventoryInfo> actualInventoryInfos = null;
            _inventoryServiceMock.Setup(x => x.UpsertInventories(It.IsAny<IEnumerable<InventoryInfo>>()))
                .Callback((IEnumerable<InventoryInfo> inventoryInfos) => { actualInventoryInfos = inventoryInfos; });

            // Act
            await _targetHandler.Handle(orderChangedEvent);

            // Assert
            Assert.Equal(expectedInventoryInfos, actualInventoryInfos, new InventoryInfoEqualityComparer());
        }

        [Fact]
        public async Task AdjustInventoryHandler_AdjustsInventory_OnOrderDelete()
        {
            // Arrange
            var oldOrder = CreateTestOrder("TESTORDER-000001");
            var changedEntries = new[] { new GenericChangedEntry<CustomerOrder>(oldOrder, oldOrder, EntryState.Deleted) };
            var orderChangedEvent = new OrderChangedEvent(changedEntries);

            var sourceInventoryInfos = new[]
            {
                new InventoryInfo
                {
                    ProductId = "shoes",
                    InStockQuantity = 3
                },
                new InventoryInfo
                {
                    ProductId = "t-shirt",
                    InStockQuantity = 0
                },
            };
            _inventoryServiceMock.Setup(x => x.GetProductsInventoryInfos(It.IsAny<IEnumerable<string>>()))
                .Returns(sourceInventoryInfos);

            var expectedInventoryInfos = new[]
            {
                new InventoryInfo
                {
                    ProductId = "shoes",
                    InStockQuantity = 5 // 2 pieces returned
                },
                new InventoryInfo
                {
                    ProductId = "t-shirt",
                    InStockQuantity = 2 // 2 pieces returned
                }
            };

            IEnumerable<InventoryInfo> actualInventoryInfos = null;
            _inventoryServiceMock.Setup(x => x.UpsertInventories(It.IsAny<IEnumerable<InventoryInfo>>()))
                .Callback((IEnumerable<InventoryInfo> inventoryInfos) => { actualInventoryInfos = inventoryInfos; });

            // Act
            await _targetHandler.Handle(orderChangedEvent);

            // Assert
            Assert.Equal(expectedInventoryInfos, actualInventoryInfos, new InventoryInfoEqualityComparer());
        }

        private static CustomerOrder CreateTestOrder(string id)
        {
            return new CustomerOrder
            {
                Id = id,
                Currency = "USD",
                CustomerId = "Test Customer",
                EmployeeId = "employee",
                StoreId = "test store",
                CreatedDate = DateTime.Now,
                Items = new List<LineItem>
                {
                    new LineItem
                    {
                        Id = "{01234567-89ab-cdef-0123-456789abcdef}",
                        Price = 20,
                        ProductId = "shoes",
                        CatalogId = "catalog",
                        Currency = "USD",
                        CategoryId = "category",
                        Name = "shoes",
                        Quantity = 2,
                        ShippingMethodCode = "EMS"
                    },
                    new LineItem
                    {
                        Id = "{abcdef01-2345-6789-abcd-ef0123456789}",
                        Price = 100,
                        ProductId = "t-shirt",
                        CatalogId = "catalog",
                        CategoryId = "category",
                        Currency = "USD",
                        Name = "t-shirt",
                        Quantity = 2,
                        ShippingMethodCode = "EMS"
                    }
                }
            };
        }
    }
}
