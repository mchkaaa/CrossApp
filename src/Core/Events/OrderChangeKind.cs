namespace CrossApp.Core.Events;

// Види змін для замовлення (як вимагає методичка для твого домену)
public enum OrderChangeKind 
{ 
    Created,       // Замовлення створено
    LineAdded,     // Додано рядок товару
    Confirmed,     // Підтверджено
    Removed        // Видалено
}