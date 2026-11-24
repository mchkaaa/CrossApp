namespace Api.Contracts;

public sealed record SaveOrderLineRequest(
    string ProductId,
    decimal Price,
    int Quantity
);

public sealed record SaveOrderRequest(
    string CustomerId,
    bool IsActive = true,
    List<SaveOrderLineRequest>? Lines = null
)
{
    public decimal CalculateTotal() =>
        Lines?.Sum(l => l.Price * l.Quantity) ?? 0m;
}