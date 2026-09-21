namespace Core.Dto;
// Поля Id, Name та Price не є nullable, 
//оскільки кожен товар обов'язково повинен мати ідентифікатор, назву та ціну. 
//Поле Note позначено як nullable (string?), тому що примітка до товару є необов'язковою інформацією
// і може бути відсутньою.
public record ProductDto(
    string Id,
    string Name,
    decimal Price,
    string? Note = null
);