using System;
using System.Collections.Generic;
using System.Linq;
using CrossApp.Core.Domain;
using Core.Domain; // НОВЕ: додано простір імен для твого класу Customer

namespace CrossApp.Core.Reports;

public static class ReportService
{
    public static IReadOnlyList<OrderLine> ExpensiveOrders(IEnumerable<Order> source, decimal threshold) =>
        source.Where(o => o.Total > threshold)
              .OrderBy(o => o.Total).ThenBy(o => o.Id, StringComparer.Ordinal)
              .Select(o => new OrderLine(o.Id, o.CustomerId, o.Total, o.Status))
              .ToList();

    // ЗАВДАННЯ 2: Використання власного методу розширення TopBy (Звіт 1)
    public static IReadOnlyList<OrderLine> TopByTotal(IEnumerable<Order> source, int n) =>
        source.TopBy(o => o.Total, n)
              .Select(o => new OrderLine(o.Id, o.CustomerId, o.Total, o.Status))
              .ToList();

    // ЗАВДАННЯ 2: Використання власного методу розширення TopBy (Звіт 2)
    public static IReadOnlyList<GroupSummary> ByStatus(IEnumerable<Order> source) =>
        source.GroupBy(o => o.Status)
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(o => o.Total)))
              .TopBy(s => s.TotalSum, 10) 
              .ToList();

    public static (int Items, decimal TotalSum, decimal Average, decimal Max) Totals(IEnumerable<Order> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0m, 0m, 0m)
            : (list.Count, list.Sum(o => o.Total), list.Average(o => o.Total), list.Max(o => o.Total));
    }

    // ЗАВДАННЯ 1: GroupJoin двох колекцій (Клієнти × Замовлення)
    public static IReadOnlyList<CustomerOrderSummary> CustomerStats(IEnumerable<Customer> customers, IEnumerable<Order> orders) =>
        customers.GroupJoin(
            orders,
            c => c.Id,          // Ключ із твого класу Customer
            o => o.CustomerId,  // Ключ із класу Order
            (c, ords) => new CustomerOrderSummary(c.Id, ords.Count(), ords.Sum(o => o.Total)) // Проєкція у зведений DTO
        ).ToList();
        
    // ЗАВДАННЯ 3: Синтаксис запитів (from ... group ... into)
    public static IReadOnlyList<GroupSummary> ByStatusQuerySyntax(IEnumerable<Order> source)
    {
        var query = from o in source
                    group o by o.Status into g
                    orderby g.Sum(x => x.Total) descending
                    select new GroupSummary(g.Key, g.Count(), g.Sum(x => x.Total));
        
        return query.ToList();
    }
}