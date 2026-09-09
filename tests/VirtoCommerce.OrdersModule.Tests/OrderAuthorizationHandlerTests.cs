using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Data.Authorization;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    [Trait("Category", "CI")]
    public class OrderAuthorizationHandlerTests
    {
        /// <summary>
        /// Exposes the protected scope-narrowing logic so it can be tested without the claims plumbing.
        /// </summary>
        private sealed class TestableOrderAuthorizationHandler(IOptions<MvcNewtonsoftJsonOptions> jsonOptions)
            : OrderAuthorizationHandler(jsonOptions)
        {
            public bool Handle(OrderOperationSearchCriteriaBase criteria, OrderAuthorizationContext context)
            {
                return HandleRequirement(criteria, context);
            }
        }

        private static TestableOrderAuthorizationHandler GetHandler()
        {
            return new TestableOrderAuthorizationHandler(Options.Create(new MvcNewtonsoftJsonOptions()));
        }

        [Fact]
        public void HandleRequirement_NoRequestedStores_NarrowsToAllowedStores()
        {
            var handler = GetHandler();
            var criteria = new CustomerOrderSearchCriteria();
            var context = new OrderAuthorizationContext { AllowedStoreIds = ["StoreA", "StoreB"] };

            var succeed = handler.Handle(criteria, context);

            Assert.True(succeed);
            Assert.Equal(["StoreA", "StoreB"], criteria.StoreIds);
        }

        [Fact]
        public void HandleRequirement_RequestedStoreWithinScope_NarrowsToIntersection()
        {
            var handler = GetHandler();
            var criteria = new CustomerOrderSearchCriteria { StoreIds = ["StoreB"] };
            var context = new OrderAuthorizationContext { AllowedStoreIds = ["StoreA", "StoreB"] };

            var succeed = handler.Handle(criteria, context);

            Assert.True(succeed);
            Assert.Equal(["StoreB"], criteria.StoreIds);
        }

        [Fact]
        public void HandleRequirement_NoStoreScope_LeavesRequestedStoresUntouched()
        {
            var handler = GetHandler();
            var criteria = new CustomerOrderSearchCriteria { StoreIds = ["StoreC"] };
            var context = new OrderAuthorizationContext { AllowedStoreIds = [] };

            var succeed = handler.Handle(criteria, context);

            Assert.True(succeed);
            Assert.Equal(["StoreC"], criteria.StoreIds);
        }

        [Fact]
        public void HandleRequirement_ResponsibleScope_SetsEmployeeId()
        {
            var handler = GetHandler();
            var criteria = new CustomerOrderSearchCriteria();
            var context = new OrderAuthorizationContext { HasResponsibleScope = true, EmployeeId = "employee1" };

            var succeed = handler.Handle(criteria, context);

            Assert.True(succeed);
            Assert.Equal("employee1", criteria.EmployeeId);
        }
    }
}
