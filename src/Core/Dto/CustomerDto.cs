namespace Core.Dto;
//поля Id, Name та Email є обов'язковими для реєстрації клієнта (не nullable). 
//Поле Phone позначено як nullable (string?), оскільки клієнт може не вказати свій номер телефону 
//під час оформлення замовлення.
public record CustomerDto(
    string Id,
    string Name,
    string Email,
    string? Phone = null
);