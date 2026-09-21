namespace Core.Dto;
// Поле Note є nullable (string?), оскільки примітка може бути відсутньою.
public record ProductDto(
    string Id,
    string Name,
    decimal Price,
    string? Note = null
);