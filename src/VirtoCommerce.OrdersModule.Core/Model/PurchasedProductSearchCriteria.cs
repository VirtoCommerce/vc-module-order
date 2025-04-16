using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Models;

public class PurchasedProductSearchCriteria : SearchCriteriaBase
{
    public string UserId { get; set; }

    public IList<string> ProductIds { get; set; }
}
