using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Services;
using Xunit;

namespace VirtoCommerce.OrdersModule.Tests
{
    /// <summary>
    /// Restoring a backup that was exported without prices must not write its zeros over the stored
    /// values. The caller doing the restore is an administrator who *can* read prices, so keying the
    /// safeguard on the caller alone leaves exactly this case unprotected - the payload has to be
    /// able to declare itself price-free, which is what OrderOperation.WithPrices does.
    /// </summary>
    [Trait("Category", "CI")]
    public class PriceFreeImportTests
    {
        private sealed class TestableDataProtectionService(ICustomerOrderService crudService, bool callerCanReadPrices)
            : CustomerOrderDataProtectionService(crudService, null, null, null, null)
        {
            protected override Task<ClaimsPrincipal> GetCurrentUser() => Task.FromResult(new ClaimsPrincipal());

            protected override Task<bool> CanReadPrices(ClaimsPrincipal user, CustomerOrder order) =>
                Task.FromResult(callerCanReadPrices);

            public Task Restore(CustomerOrder order) => RestoreDetailsForUser(null, order);
        }

        private static CustomerOrder StoredOrder() => new()
        {
            Id = "orderId",
            Total = 555.55m,
            SubTotal = 555.55m,
            Sum = 555.55m,
            Items = [new LineItem { Id = "itemId", Price = 555.55m, PlacedPrice = 555.55m }],
        };

        /// <summary>An order as it comes out of a price-free archive.</summary>
        private static CustomerOrder ImportedPriceFreeOrder() => new()
        {
            Id = "orderId",
            WithPrices = false,
            Total = 0m,
            SubTotal = 0m,
            Sum = 0m,
            Items = [new LineItem { Id = "itemId", Price = 0m, PlacedPrice = 0m }],
        };

        private static TestableDataProtectionService GetService(bool callerCanReadPrices, CustomerOrder stored)
        {
            var crud = new Mock<ICustomerOrderService>();
            crud.Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync((IList<string> ids, string _, bool __) =>
                    stored != null && ids.Contains(stored.Id) ? [stored] : []);

            return new TestableDataProtectionService(crud.Object, callerCanReadPrices);
        }

        [Fact]
        public async Task Restore_PriceFreeArchive_AsEntitledCaller_DoesNotZeroStoredPrices()
        {
            var imported = ImportedPriceFreeOrder();

            // An administrator restoring a backup: fully entitled to read prices
            await GetService(callerCanReadPrices: true, StoredOrder()).Restore(imported);

            Assert.Equal(555.55m, imported.Total);
            Assert.Equal(555.55m, imported.SubTotal);
            Assert.Equal(555.55m, imported.Items.First().Price);
            Assert.True(imported.WithPrices);
        }

        [Fact]
        public async Task Restore_PriceFreeArchive_AsRestrictedCaller_AlsoRestores()
        {
            var imported = ImportedPriceFreeOrder();

            await GetService(callerCanReadPrices: false, StoredOrder()).Restore(imported);

            Assert.Equal(555.55m, imported.Total);
        }

        [Fact]
        public async Task Restore_FaithfulArchive_AsEntitledCaller_WritesArchiveValuesUnchanged()
        {
            // A normal backup carries WithPrices = true, so a legitimate restore must not be rewritten
            var imported = new CustomerOrder { Id = "orderId", WithPrices = true, Total = 999m, SubTotal = 999m };

            await GetService(callerCanReadPrices: true, StoredOrder()).Restore(imported);

            Assert.Equal(999m, imported.Total);
            Assert.Equal(999m, imported.SubTotal);
        }

        [Fact]
        public async Task Restore_PriceFreeOrderNotInStorage_LeavesZerosAndDestroysNothing()
        {
            var imported = ImportedPriceFreeOrder();

            // Nothing stored under that id: a brand new order carried by the archive
            await GetService(callerCanReadPrices: true, stored: null).Restore(imported);

            Assert.Equal(0m, imported.Total);
        }

        [Fact]
        public async Task WithPrices_DefaultsToTrue_SoOrdinaryApiWritesAreNeverRewritten()
        {
            // Clients that omit the field must not be treated as price-free
            Assert.True(new CustomerOrder().WithPrices);
            Assert.True(new PaymentIn().WithPrices);
            Assert.True(new Shipment().WithPrices);
        }
    }
}
