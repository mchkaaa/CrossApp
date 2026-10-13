using CrossApp.Core.Domain;

namespace CrossApp.Core.Dto;

// Обов'язково public, щоб FileOrderStore міг з ним працювати
public record OrderDto(string Id, string CustomerId, bool IsActive, OrderStatus Status);