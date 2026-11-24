using CrossApp.Core.Domain;

namespace Api.Contracts;

public sealed record OrderLineDto(string ProductId, decimal Price, int Quantity);

public sealed record OrderResponseDto(
    string Id,
    string CustomerId,
    bool IsActive,
    string Status,
    decimal Total,
    IReadOnlyList<OrderLineDto> Lines
);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Page,
    int PageSize
);

public static class OrderMappings
{
    public static OrderResponseDto ToApiDto(this Order order, IReadOnlyList<OrderLineDto>? lines = null) =>
        new(
            order.Id,
            order.CustomerId,
            order.IsActive,
            order.Status.ToString(),
            order.Total,
            lines ?? []
        );
}