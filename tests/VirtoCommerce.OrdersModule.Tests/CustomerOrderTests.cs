using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using Xunit;


namespace VirtoCommerce.OrdersModule.Tests
{
    public class CustomerOrderTests
    {
        [Fact]
        public void CloneTest()
        {
            // Arrange
            var shipment = new Shipment();

            var order = new CustomerOrder
            {
                Shipments = [shipment],
                ChildrenOperations = [shipment],
            };

            // Act
            var clonedOrder = order.CloneTyped();

            // Assert
            Assert.NotSame(clonedOrder.Shipments, order.Shipments);
            Assert.NotSame(clonedOrder.Shipments.First(), order.Shipments.First());

            Assert.NotSame(clonedOrder.ChildrenOperations, order.ChildrenOperations);
            Assert.NotSame(clonedOrder.ChildrenOperations.First(), order.ChildrenOperations.First());

            Assert.Same(clonedOrder.Shipments.First(), clonedOrder.ChildrenOperations.First());
        }

        [Fact]
        public void FromModel_MultipleShipments_ShipmentItemsLinkedCorrectly()
        {
            // Arrange
            // two line items, two shipments, each shipment has one shipmentItem corresponding with order lineItem
            var lineItem1 = new LineItem
            {
                ProductId = Guid.NewGuid().ToString(),
                Quantity = 2
            };
            var lineItem2 = new LineItem
            {
                ProductId = Guid.NewGuid().ToString(),
                Quantity = 5
            };

            var order = new CustomerOrder
            {
                Items = new List<LineItem>
                {
                    lineItem1,
                    lineItem2
                },
                Shipments = new List<Shipment>
                {
                    new Shipment
                    {
                        Items = new List<ShipmentItem>
                        {
                            new ShipmentItem
                            {
                                LineItem = lineItem1
                            }
                        }
                    },
                    new Shipment()
                    {
                        Items = new List<ShipmentItem>
                        {
                            new ShipmentItem
                            {
                                LineItem = lineItem2
                            }
                        }
                    }
                }
            };

            // Act
            var orderEntity = AbstractTypeFactory<CustomerOrderEntity>
                .TryCreateInstance()
                .FromModel(order, new PrimaryKeyResolvingMap()) as CustomerOrderEntity;

            // Assert
            var shipmentLineItem1 = orderEntity.Shipments.First().Items.First().LineItem;
            var shipmentLineItem2 = orderEntity.Shipments.Last().Items.First().LineItem;
            var equals = ReferenceEquals(shipmentLineItem1, shipmentLineItem2);
            Assert.False(equals);
        }
    }
}
