namespace CrossApp.Core.Events;

// Види змін для замовлення 
public enum OrderChangeKind 
{ 
    Created,       // Замовлення створено
    LineAdded,     // Додано рядок товару
    Confirmed,     // Підтверджено
    Removed        // Видалено
}