using CrossApp.Core.Domain;

namespace CrossApp.Core.Dto;

public sealed record OrderDto(string Id, string CustomerId, bool IsActive, OrderStatus Status, decimal Total);