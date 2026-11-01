namespace CrossApp.Core.Reports;

public sealed record CustomerOrderSummary(string CustomerId, int OrdersCount, decimal TotalSum);