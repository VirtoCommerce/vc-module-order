using System;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    public interface ICustomerOrderStatisticService
    {
        /// <summary>
        /// Calculates various order statistics for a dashboard based on the provided start and end dates.
        /// </summary>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <returns>Returns a DashboardStatisticsResult object that encapsulates the computed statistics.</returns>
        Task<DashboardStatisticsResult> CollectStatisticsAsync(DateTime start, DateTime end);
    }
}
