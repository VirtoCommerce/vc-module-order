using System;
using System.Collections.Generic;
using Moq;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.OrderModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.OrderModule.Test
{
    [CLSCompliant(false)]
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
                    Id = "{01234567-89ab-cdef-0123-456789abcdef}",
                    ProductId = "shoes",
                    Quantity = 2
                },
                new LineItem
                {
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
                    Id = "{01234567-89ab-cdef-0123-456789abcdef}",
                    ProductId = "shoes",
                    Quantity = 5
                },
                new LineItem
                {
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
                    new AdjustInventoryOrderChangedEventHandler.ProductInventoryChange
                    {
                        FulfillmentCenterId = TestFulfillmentCenterId,
                        ProductId = "shoes",
                        QuantityDelta = 3
                    },
                    new AdjustInventoryOrderChangedEventHandler.ProductInventoryChange
                    {
                        FulfillmentCenterId = TestFulfillmentCenterId,
                        ProductId = "t-shirt-new",
                        QuantityDelta = 1
                    },
                    new AdjustInventoryOrderChangedEventHandler.ProductInventoryChange
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
                    new AdjustInventoryOrderChangedEventHandler.ProductInventoryChange
                    {
                        FulfillmentCenterId = TestFulfillmentCenterId,
                        ProductId = "shoes",
                        QuantityDelta = 2
                    },
                    new AdjustInventoryOrderChangedEventHandler.ProductInventoryChange
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
                    new AdjustInventoryOrderChangedEventHandler.ProductInventoryChange
                    {
                        FulfillmentCenterId = TestFulfillmentCenterId,
                        ProductId = "shoes",
                        QuantityDelta = -2
                    },
                    new AdjustInventoryOrderChangedEventHandler.ProductInventoryChange
                    {
                        FulfillmentCenterId = TestFulfillmentCenterId,
                        ProductId = "t-shirt",
                        QuantityDelta = -2
                    }
                }
            }
        };

        private bool CompareProductInventoryChanges(
            AdjustInventoryOrderChangedEventHandler.ProductInventoryChange first,
            AdjustInventoryOrderChangedEventHandler.ProductInventoryChange second)
        {
            if (ReferenceEquals(first, second))
                return true;

            if (first == null || second == null)
                return false;

            return first.FulfillmentCenterId == second.FulfillmentCenterId &&
                   first.ProductId == second.ProductId &&
                   first.QuantityDelta == second.QuantityDelta;
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void AdjustInventoryHandler_GetProductInventoryChanges_ForOrderChanges(GenericChangedEntry<CustomerOrder> orderChangedEntry,
            IEnumerable<AdjustInventoryOrderChangedEventHandler.ProductInventoryChange> expectedChanges)
        {
            // Arrange
            var inventoryServiceMock = new Mock<IInventoryService>();
            var settingsManagerMock = new Mock<ISettingsManager>();

            var storeServiceMock = new Mock<IStoreService>();
            storeServiceMock.Setup(x => x.GetById(TestStoreId))
                .Returns(new Store { MainFulfillmentCenterId = TestFulfillmentCenterId });

            var targetHandler = new AdjustInventoryOrderChangedEventHandler(inventoryServiceMock.Object,
                storeServiceMock.Object, settingsManagerMock.Object);

            // Act
            var actualChanges = targetHandler.GetProductInventoryChangesFor(orderChangedEntry);
            
            // Assert
            var equalityComparer =
                AnonymousComparer.Create<AdjustInventoryOrderChangedEventHandler.ProductInventoryChange>(
                    CompareProductInventoryChanges,
                    change => change.GetHashCode());
            Assert.Equal(expectedChanges, actualChanges, equalityComparer);
        }
    }
}
