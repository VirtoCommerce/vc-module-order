using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Repositories;

namespace VirtoCommerce.OrdersModule.Data.Services
{
    public class CustomerOrderStatisticService : ICustomerOrderStatisticService
    {
        private readonly Func<IOrderRepository> _repositoryFactory;

        public CustomerOrderStatisticService(Func<IOrderRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        /// <summary>
        /// Calculates various order statistics for a dashboard based on the provided start and end dates.
        /// </summary>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <returns>Returns a DashboardStatisticsResult object that encapsulates the computed statistics.</returns>
        public async Task<DashboardStatisticsResult> CollectStatisticsAsync(DateTime start, DateTime end)
        {
            var retVal = new DashboardStatisticsResult
            {
                StartDate = start,
                EndDate = end,
                RevenuePeriodDetails = new List<QuarterPeriodMoney>(),
                AvgOrderValuePeriodDetails = new List<QuarterPeriodMoney>()
            };

            using (var repository = _repositoryFactory())
            {
                retVal.OrderCount = await CalculateOrderCount(repository, start, end);

                retVal.AvgOrderValue = await CalculateAvgOrderValue(repository, start, end);

                retVal.Revenue = await CalculateRevenue(repository, start, end);

                retVal.RevenuePerCustomer = await CalculateRevenueRevenuePerCustomer(repository, start, end);

                retVal.ItemsPurchased = await CalculateItemsPurchased(repository, start, end);

                retVal.LineitemsPerOrder = await CalculateLineItemsPerOrder(repository, start, end);

                retVal.CustomersCount = await CalculateCustomersCount(repository, start, end);

                await CalculateMetricsPerQuarter(repository, start, end, retVal);
            }

            return retVal;
        }

        protected virtual Task<int> CalculateCustomersCount(IOrderRepository repository, DateTime start, DateTime end)
        {
            return repository.CustomerOrders.Where(x => x.CreatedDate >= start && x.CreatedDate <= end)
                       .Select(x => x.CustomerId).Distinct().CountAsync();
        }

        protected virtual async Task<double> CalculateLineItemsPerOrder(IOrderRepository repository, DateTime start, DateTime end)
        {
            var itemsCount = await repository.CustomerOrders.Where(x => x.CreatedDate >= start && x.CreatedDate <= end)
                .Where(x => !x.IsCancelled).Select(x => x.Items.Count).ToArrayAsync();
            var lineitemsPerOrder = itemsCount.Any() ? itemsCount.DefaultIfEmpty(0).Average() : 0;
            return lineitemsPerOrder;
        }

        protected virtual Task<int> CalculateItemsPurchased(IOrderRepository repository, DateTime start, DateTime end)
        {
            return repository.CustomerOrders.Where(x => x.CreatedDate >= start && x.CreatedDate <= end)
                                                                                .Where(x => !x.IsCancelled).SelectMany(x => x.Items).CountAsync();
        }

        protected virtual Task<List<Money>> CalculateRevenueRevenuePerCustomer(IOrderRepository repository, DateTime start, DateTime end)
        {
            var revenues = repository.InPayments.Where(x => x.CreatedDate >= start && x.CreatedDate <= end && !x.IsCancelled)
                                        .GroupBy(x => new { x.Currency, x.CustomerId }, (key, result) => new { key.Currency, key.CustomerId, Sum = result.Sum(y => y.Sum) })
                                        .GroupBy(x => x.Currency, (key, result) =>
                                            new
                                            {
                                                Currency = key,
                                                AvgValue = result.Average(x => x.Sum)
                                            });
            return revenues.Select(x => new Money(x.Currency, x.AvgValue)).ToListAsync();
        }

        protected virtual async Task CalculateMetricsPerQuarter(IOrderRepository repository, DateTime start, DateTime end, DashboardStatisticsResult retVal)
        {
            var currencies = await repository.InPayments.Where(x => x.CreatedDate >= start && x.CreatedDate <= end)
                        .Where(x => !x.IsCancelled)
                        .GroupBy(x => x.Currency, (key, result) => key).ToListAsync();

            DateTime endDate;
            foreach (var currency in currencies)
            {
                for (var startDate = start; startDate < end; startDate = endDate)
                {
                    endDate = startDate.AddMonths(3 - ((startDate.Month - 1) % 3));
                    endDate = new DateTime(endDate.Year, endDate.Month, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    endDate = new DateTime(Math.Min(end.Ticks, endDate.Ticks), DateTimeKind.Utc);
                    var quarter = (startDate.Month - 1) / 3 + 1;

                    var amount = await CalculateOrderAmount(startDate, endDate, currency, repository);

                    var avgOrderValue = await CalculateAvgOrderValue(startDate, endDate, currency, repository);

                    var periodStat = new QuarterPeriodMoney(currency, amount)
                    {
                        Quarter = quarter,
                        Year = startDate.Year
                    };
                    retVal.RevenuePeriodDetails.Add(periodStat);

                    periodStat = new QuarterPeriodMoney(currency, avgOrderValue)
                    {
                        Quarter = quarter,
                        Year = startDate.Year
                    };
                    retVal.AvgOrderValuePeriodDetails.Add(periodStat);


                }
            }
        }

        protected virtual Task<decimal> CalculateOrderAmount(DateTime startDate, DateTime endDate, string currency, IOrderRepository repository)
        {
            return repository.InPayments.Where(x => x.CreatedDate >= startDate && x.CreatedDate <= endDate && !x.IsCancelled && x.Currency == currency)
                            .GroupBy(x => 1, (key, result) => result.Sum(x => x.Sum)).FirstOrDefaultAsync();
        }

        protected virtual async Task<List<Money>> CalculateRevenue(IOrderRepository repository, DateTime start, DateTime end)
        {
            var revenues = await repository.InPayments.Where(x => x.CreatedDate >= start && x.CreatedDate <= end)
                                        .Where(x => !x.IsCancelled)
                                        .GroupBy(x => x.Currency, (key, result) => new { Currency = key, Value = result.Sum(y => y.Sum) })
                                        .ToArrayAsync();
            var revenueMoney = revenues.Select(x => new Money(x.Currency, x.Value)).ToList();
            return revenueMoney;
        }

        protected virtual Task<int> CalculateOrderCount(IOrderRepository repository, DateTime start, DateTime end)
        {
            return repository.CustomerOrders.CountAsync(x => x.CreatedDate >= start && x.CreatedDate <= end && !x.IsCancelled);
        }

        protected virtual async Task<List<Money>> CalculateAvgOrderValue(IOrderRepository repository, DateTime start, DateTime end)
        {
            var avgValues = await repository.CustomerOrders.Where(x => x.CreatedDate >= start && x.CreatedDate <= end)
                .Select(o => new { Currency = o.Currency, Total = o.Total })
                .GroupBy(x => x.Currency, (key, result) => new { Currency = key, AvgValue = result.Average(y => y.Total) })
                .ToArrayAsync();
            var avgOrderValue = avgValues.Select(x => new Money(x.Currency, x.AvgValue)).ToList();
            return avgOrderValue;
        }

        protected virtual Task<decimal> CalculateAvgOrderValue(DateTime start, DateTime end, string currency, IOrderRepository repository)
        {
            return repository.CustomerOrders.Where(x => x.CreatedDate >= start && x.CreatedDate <= end && x.Currency == currency)
                            .GroupBy(x => 1, (key, result) => result.Average(x => x.Total)).FirstOrDefaultAsync();
        }
    }
}
