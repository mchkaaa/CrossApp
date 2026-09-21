namespace Core.Dto;

// Використовуємо узагальнений тип <T>, щоб цей результат міг працювати
// як з ProductDto, так і з CustomerDto чи будь-яким іншим типом.
public sealed record ImportResult<T>(
    IReadOnlyList<T> Items,
    IReadOnlyList<string> Errors
);