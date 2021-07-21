using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.OrdersModule.Web.Model;

namespace VirtoCommerce.OrdersModule.Web.BackgroundJobs
{
    public class CollectOrderStatisticJob
    {
        private readonly Func<IOrderRepository> _repositoryFactory;

        internal CollectOrderStatisticJob()
        {
        }

        public CollectOrderStatisticJob(Func<IOrderRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public async Task<DashboardStatisticsResult> CollectStatisticsAsync(DateTime start, DateTime end)
        {
            //var retVal = new DashboardStatisticsResult();

            //using (var repository = _repositoryFactory())
            //{
            //    var currencies = repository.InPayments.Where(x => x.CreatedDate >= start && x.CreatedDate <= end)
            //                            .Where(x => !x.IsCancelled)
            //                            .GroupBy(x => x.Currency, (key, result) => key).ToList();

            //    retVal.OrderCount = await repository.CustomerOrders.CountAsync(x => x.CreatedDate >= start && x.CreatedDate <= end && !x.IsCancelled);
            //    //avg order value
            //    var avgValues = await repository.CustomerOrders.Where(x => x.CreatedDate >= start && x.CreatedDate <= end)
            //                                             .GroupBy(x => x.Currency, (key, result) => new { Currency = key, AvgValue = result.Average(y => y.Total) })
            //                                             .ToArrayAsync();
            //    retVal.AvgOrderValue = avgValues.Select(x => new Money(x.Currency, x.AvgValue)).ToList();


            //    //Revenue
            //    var revenues = await repository.InPayments.Where(x => x.CreatedDate >= start && x.CreatedDate <= end)
            //                                        .Where(x => !x.IsCancelled)
            //                                        .GroupBy(x => x.Currency, (key, result) => new { Currency = key, Value = result.Sum(y => y.Sum) })
            //                                        .ToArrayAsync();
            //    retVal.Revenue = revenues.Select(x => new Money(x.Currency, x.Value)).ToList();


            //    retVal.RevenuePeriodDetails = new List<QuarterPeriodMoney>();
            //    retVal.AvgOrderValuePeriodDetails = new List<QuarterPeriodMoney>();
            //    DateTime endDate;
            //    foreach (var currency in currencies)
            //    {
            //        for (var startDate = start; startDate < end; startDate = endDate)
            //        {
            //            endDate = startDate.AddMonths(3 - ((startDate.Month - 1) % 3));
            //            endDate = new DateTime(endDate.Year, endDate.Month, 1);
            //            endDate = new DateTime(Math.Min(end.Ticks, endDate.Ticks));
            //            var quarter = (startDate.Month - 1) / 3 + 1;

            //            var amount = await repository.InPayments.Where(x => x.CreatedDate >= startDate && x.CreatedDate <= endDate && !x.IsCancelled && x.Currency == currency)
            //                                             .GroupBy(x => 1, (key, result) => result.Sum(x => x.Sum)).FirstOrDefaultAsync();

            //            var avgOrderValue = await repository.CustomerOrders.Where(x => x.CreatedDate >= startDate && x.CreatedDate <= endDate && x.Currency == currency)
            //                                             .GroupBy(x => 1, (key, result) => result.Average(x => x.Total)).FirstOrDefaultAsync();

            //            var periodStat = new QuarterPeriodMoney(currency, amount)
            //            {
            //                Quarter = quarter,
            //                Year = startDate.Year
            //            };
            //            retVal.RevenuePeriodDetails.Add(periodStat);

            //            periodStat = new QuarterPeriodMoney(currency, avgOrderValue)
            //            {
            //                Quarter = quarter,
            //                Year = startDate.Year
            //            };
            //            retVal.AvgOrderValuePeriodDetails.Add(periodStat);


            //        }
            //    }

            //    //RevenuePerCustomer
            //    var revenuesPerCustomer = repository.InPayments.Where(x => x.CreatedDate >= start && x.CreatedDate <= end && !x.IsCancelled)
            //                                .GroupBy(x => new { x.Currency, x.CustomerId }, (key, result) => new { key.Currency, key.CustomerId, Sum = result.Sum(y => y.Sum) })
            //                                .GroupBy(x => x.Currency, (key, result) => new { Currency = key, AvgValue = result.Average(x => x.Sum) });

            //    retVal.RevenuePerCustomer = revenuesPerCustomer.Select(x => new Money(x.Currency, x.AvgValue)).ToList();

            //    //Items purchased
            //    retVal.ItemsPurchased = await repository.CustomerOrders.Where(x => x.CreatedDate >= start && x.CreatedDate <= end)
            //                                                        .Where(x => !x.IsCancelled).SelectMany(x => x.Items).CountAsync();

            //    //Line items per order
            //    var itemsCount = await repository.CustomerOrders.Where(x => x.CreatedDate >= start && x.CreatedDate <= end)
            //        .Where(x => !x.IsCancelled).Select(x => x.Items.Count).ToArrayAsync();
            //    retVal.LineitemsPerOrder = itemsCount.Any() ? itemsCount.DefaultIfEmpty(0).Average() : 0;

            //    //Customer count
            //    retVal.CustomersCount = await repository.CustomerOrders.Where(x => x.CreatedDate >= start && x.CreatedDate <= end)
            //                                                        .Select(x => x.CustomerId).Distinct().CountAsync();

            //}
            //retVal.StartDate = start;
            //retVal.EndDate = end;
            //return retVal;
            return null;
        }
    }
}
