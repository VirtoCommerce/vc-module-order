using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Data.Authorization;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    /// <summary>
    /// Store scope narrowing. The search services (SQL and indexed alike) treat an empty StoreIds as
    /// "no store filter", so the scope must never leave it empty: doing so widens a scoped user's
    /// search to every store instead of narrowing it.
    /// </summary>
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

        private static OrderAuthorizationContext ElectronicsScope()
        {
            return new OrderAuthorizationContext { AllowedStoreIds = ["Electronics"] };
        }

        [Fact]
        public void NoRequestedStores_NarrowsToScope()
        {
            var criteria = new CustomerOrderSearchCriteria();

            var succeed = GetHandler().Handle(criteria, ElectronicsScope());

            Assert.True(succeed);
            Assert.Equal(["Electronics"], criteria.StoreIds);
        }

        [Fact]
        public void RequestedStoreInScope_NarrowsToScope()
        {
            var criteria = new CustomerOrderSearchCriteria { StoreIds = ["Electronics"] };

            var succeed = GetHandler().Handle(criteria, ElectronicsScope());

            Assert.True(succeed);
            Assert.Equal(["Electronics"], criteria.StoreIds);
        }

        [Fact]
        public void RequestedStoresPartlyInScope_KeepsOnlyThePermittedOne()
        {
            var criteria = new CustomerOrderSearchCriteria { StoreIds = ["B2B-store", "Electronics"] };

            var succeed = GetHandler().Handle(criteria, ElectronicsScope());

            Assert.True(succeed);
            Assert.Equal(["Electronics"], criteria.StoreIds);
        }

        [Fact]
        public void RequestedStoreDiffersOnlyInCase_StillMatchesScope()
        {
            var criteria = new CustomerOrderSearchCriteria { StoreIds = ["ELECTRONICS"] };

            var succeed = GetHandler().Handle(criteria, ElectronicsScope());

            Assert.True(succeed);
            Assert.Equal(["Electronics"], criteria.StoreIds);
        }

        [Theory]
        [InlineData("B2B-store")]   // a real store, outside the scope
        [InlineData("NoSuchStore")] // a store that does not exist
        public void RequestedStoreOutsideScope_FiltersToNothingRatherThanEverything(string requestedStore)
        {
            var criteria = new CustomerOrderSearchCriteria { StoreIds = [requestedStore] };

            var succeed = GetHandler().Handle(criteria, ElectronicsScope());

            // An empty StoreIds would be read downstream as "no store filter" and return every store
            Assert.NotEmpty(criteria.StoreIds);
            Assert.DoesNotContain(requestedStore, criteria.StoreIds);
            Assert.DoesNotContain("Electronics", criteria.StoreIds);
            Assert.True(succeed);
        }

        [Fact]
        public void NoStoreScope_LeavesRequestedStoresUntouched()
        {
            var criteria = new CustomerOrderSearchCriteria { StoreIds = ["B2B-store"] };

            var succeed = GetHandler().Handle(criteria, new OrderAuthorizationContext { AllowedStoreIds = [] });

            Assert.True(succeed);
            Assert.Equal(["B2B-store"], criteria.StoreIds);
        }

        [Fact]
        public void ResponsibleScope_SetsEmployeeId()
        {
            var criteria = new CustomerOrderSearchCriteria();

            var succeed = GetHandler().Handle(criteria, new OrderAuthorizationContext { HasResponsibleScope = true, EmployeeId = "employee1" });

            Assert.True(succeed);
            Assert.Equal("employee1", criteria.EmployeeId);
        }

        /// <summary>
        /// The indexed endpoint uses the same handler path, so the scope must reach it identically.
        /// </summary>
        [Fact]
        public void IndexedCriteria_OutsideScope_FiltersToNothingRatherThanEverything()
        {
            var criteria = new CustomerOrderIndexedSearchCriteria { StoreIds = ["B2B-store"] };

            var succeed = GetHandler().Handle(criteria, ElectronicsScope());

            Assert.True(succeed);
            Assert.NotEmpty(criteria.StoreIds);
            Assert.DoesNotContain("B2B-store", criteria.StoreIds);
        }
    }
}
