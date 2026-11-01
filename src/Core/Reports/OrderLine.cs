using CrossApp.Core.Domain;

namespace CrossApp.Core.Reports;

public sealed record OrderLine(string OrderId, string CustomerId, decimal Total, OrderStatus Status);