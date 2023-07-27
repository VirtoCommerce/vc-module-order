using Microsoft.EntityFrameworkCore;
using Npgsql;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.OrdersModule.Data.Services;

namespace VirtoCommerce.OrdersModule.Data.PostgreSql
{
    public class CurrencyAverage
    {
        public string Currency { get; set; }
        public decimal AverageValue { get; set; }
    }

    public class PostgreSqlCustomerOrderStatisticService : CustomerOrderStatisticService
    {
        private readonly OrderDbContext _dbContext;

        public PostgreSqlCustomerOrderStatisticService(Func<IOrderRepository> repositoryFactory, OrderDbContext dbContext) : base(repositoryFactory)
        {
            _dbContext = dbContext;
        }

        private async Task<List<CurrencyAverage>> ExecuteRawSqlQuery(string sqlQuery, NpgsqlParameter[] parameters)
        {
            var results = new List<CurrencyAverage>();
            using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sqlQuery;
                command.Parameters.AddRange(parameters);
                await _dbContext.Database.OpenConnectionAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var currencyAverage = new CurrencyAverage
                        {
                            Currency = reader.GetString(0),
                            AverageValue = reader.GetDecimal(1)
                        };
                        results.Add(currencyAverage);
                    }
                }
            }

            return results;
        }

        protected override async Task<List<DashboardMoney>> CalculateAvgOrderValue(IOrderRepository repository, DateTime start, DateTime end)
        {
            var sqlQuery = @"SELECT ""Currency"", CAST(avg(CAST(""Total"" AS numeric)) AS money) as ""AverageValue""
                   FROM ""CustomerOrder""
                   WHERE ""CreatedDate"" >= @p0 AND ""CreatedDate"" <= @p1
                   GROUP by ""Currency""
                ";

            var parameters = new[]
            {
                new NpgsqlParameter("p0", start),
                new NpgsqlParameter("p1", end)
            };

            var results = await ExecuteRawSqlQuery(sqlQuery, parameters);

            return results.Select(x => new DashboardMoney(x.Currency, x.AverageValue)).ToList();
        }

        protected override async Task<decimal> CalculateAvgOrderValue(DateTime start, DateTime end, string currency, IOrderRepository repository)
        {
            var sqlQuery = @"SELECT ""Currency"", CAST(avg(CAST(""Total"" AS numeric)) AS money) as ""AverageValue""
                   FROM ""CustomerOrder""
                   WHERE ""CreatedDate"" >= @p0 AND ""CreatedDate"" <= @p1 AND ""Currency"" = @p2
                   GROUP by ""Currency""
                ";

            var parameters = new[]
            {
                new NpgsqlParameter("p0", start),
                new NpgsqlParameter("p1", end),
                new NpgsqlParameter("p2", currency)
            };

            var results = await ExecuteRawSqlQuery(sqlQuery, parameters);

            return results.FirstOrDefault()?.AverageValue ?? 0;
        }

        protected override async Task<List<DashboardMoney>> CalculateRevenueRevenuePerCustomer(IOrderRepository repository, DateTime start, DateTime end)
        {
            var sqlQuery = @"SELECT ""Currency"", CAST(avg(CAST(""Sum"" AS numeric)) AS money) AS ""AverageValue""
                        FROM
                            (SELECT ""Currency"", SUM(""Sum"") AS ""Sum""
                            FROM ""OrderPaymentIn""
                            WHERE  
                             ""CreatedDate"" >= @p0 AND ""CreatedDate"" <= @p1 AND ""IsCancelled"" = false
                            GROUP by ""Currency"", ""CustomerId"") AS P
                        GROUP by ""Currency""
                ";

            var parameters = new[]
            {
                new NpgsqlParameter("p0", start),
                new NpgsqlParameter("p1", end)
            };

            var results = await ExecuteRawSqlQuery(sqlQuery, parameters);

            return results.Select(x => new DashboardMoney(x.Currency, x.AverageValue)).ToList();
        }
    }
}
