using CrossApp.Core.Domain;

namespace CrossApp.Core.Reports;

// Замінюємо string Key на OrderStatus Status
public sealed record GroupSummary(OrderStatus Status, int Count, decimal TotalSum);