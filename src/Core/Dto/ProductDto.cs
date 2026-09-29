namespace Core.Dto;

public record ProductDto(
    string Id,
    string Name,
    decimal Price,
    int Quantity, // Додали поле для залишку
    string? Note = null
);