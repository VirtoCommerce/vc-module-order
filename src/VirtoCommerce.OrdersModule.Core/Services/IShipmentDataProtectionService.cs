namespace VirtoCommerce.OrdersModule.Core.Services;

/// <summary>
/// Drop-in replacement for <see cref="IShipmentService"/> and <see cref="IShipmentSearchService"/> that removes
/// prices the current user may not read, and restores them before a save so that an operation received
/// without prices cannot overwrite the stored ones.
/// Resolve this instead of the raw services on any path that serves or persists shipments for a signed-in user.
/// </summary>
public interface IShipmentDataProtectionService : IShipmentSearchService, IShipmentService
{
}
