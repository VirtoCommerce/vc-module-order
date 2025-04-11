using System.Collections.Generic;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.OrdersModule.Core.Events;

public class PurchasedProductChangedEvent(IEnumerable<GenericChangedEntry<PurchasedProduct>> changedEntries)
    : GenericChangedEntryEvent<PurchasedProduct>(changedEntries);
