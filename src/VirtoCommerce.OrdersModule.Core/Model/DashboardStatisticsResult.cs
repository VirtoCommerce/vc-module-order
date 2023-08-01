using System;
using System.Collections.Generic;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class DashboardStatisticsResult
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<DashboardMoney> Revenue { get; set; }
        public ICollection<QuarterPeriodMoney> RevenuePeriodDetails { get; set; }
        public int OrderCount { get; set; }
        public int CustomersCount { get; set; }
        public ICollection<DashboardMoney> RevenuePerCustomer { get; set; }
        public ICollection<DashboardMoney> AvgOrderValue { get; set; }
        public ICollection<QuarterPeriodMoney> AvgOrderValuePeriodDetails { get; set; }

        public int ItemsPurchased { get; set; }
        public double LineItemsPerOrder { get; set; }
    }

    public class DashboardMoney
    {
        public DashboardMoney(string currency, decimal amount)
        {
            Currency = currency;
            Amount = amount;
        }

        public string Currency { get; set; }
        public decimal Amount { get; set; }
    }


    public class QuarterPeriodMoney : DashboardMoney
    {
        public QuarterPeriodMoney(string currency, decimal amount)
            : base(currency, amount)
        {
        }

        public int Year { get; set; }
        public int Quarter { get; set; }
    }
}
