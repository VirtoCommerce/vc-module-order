using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    public class CancelPaymentOrderTests
    {
        [Fact]
        public void GetJobArgumentsForChangedEntry_PaymentsCancelledStateChanged()
        {
            Assert.Fail("FAILED TEST");
            // Arrange
            var paymentId = Guid.NewGuid().ToString();
            var oldPayment = new PaymentIn
            {
                Id = paymentId,
                CancelledState = CancelledState.Undefined,
            };
            var newPayment = new PaymentIn
            {
                Id = paymentId,
                CancelledState = CancelledState.Requested,
            };
            var oldOrder = new CustomerOrder
            {
                InPayments = new List<PaymentIn>() { oldPayment },
            };
            var newOrder = new CustomerOrder
            {
                InPayments = new List<PaymentIn>() { newPayment },
            };
            var changedEntry = new GenericChangedEntry<CustomerOrder>(newOrder, oldOrder, EntryState.Modified);

            var customerOrderServiceMock = new Mock<ICustomerOrderService>();
            var handler = new CancelPaymentOrderChangedEventHandlerMock(customerOrderServiceMock.Object);

            // Act
            var result = handler.GetJobArgumentsForChangedEntryMock(changedEntry);

            // Assert
            result.Should().HaveCount(1);
            result.First().PaymentId.Should().Be(paymentId);
        }


        /// <summary>
        /// Mock class to expose protected methods for testing
        /// </summary>
        public class CancelPaymentOrderChangedEventHandlerMock : CancelPaymentOrderChangedEventHandler
        {
            public CancelPaymentOrderChangedEventHandlerMock(ICustomerOrderService customerOrderService) : base(customerOrderService)
            {
            }

            public PaymentToCancelJobArgument[] GetJobArgumentsForChangedEntryMock(GenericChangedEntry<CustomerOrder> changedEntry)
            {
                return base.GetJobArgumentsForChangedEntry(changedEntry);
            }
        }
    }
}
