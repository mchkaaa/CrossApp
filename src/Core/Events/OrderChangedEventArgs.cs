using System;

namespace CrossApp.Core.Events;

public sealed class OrderChangedEventArgs(
    string orderId, OrderChangeKind kind, int linesCount, decimal total) : EventArgs  //порожній, для дотримання стандарту
{
    public string OrderId { get; } = orderId;
    public OrderChangeKind Kind { get; } = kind;
    public int LinesCount { get; } = linesCount; // Скільки товарів у замовленні
    public decimal Total { get; } = total;       // Загальна сума
    public DateTimeOffset At { get; } = DateTimeOffset.Now; // Час події
}