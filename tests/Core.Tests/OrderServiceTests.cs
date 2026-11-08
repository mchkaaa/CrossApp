using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using CrossApp.Core.Domain;
using CrossApp.Core.Services;
using Core.Storage; 
using CrossApp.Core.Reports;
using CrossApp.Core;
using CrossApp.Core.Events;
using Core.Abstractions;
using System.Runtime.CompilerServices;

namespace Core.Tests;

public class OrderServiceTests
{
    // 1) Інваріант: відмова і незмінний стан (перевірка DomainException)
    [Fact]
    public async Task Import_ToConfirmedOrder_ThrowsDomainException_AndKeepsState()
    {
        // Arrange
        var store = new InMemoryOrderStore();
        var service = new OrderService(store, new DummySink());
        
        var order = Order.Create("ORD-1", "CUST-1", 1000m);
        order.ChangeStatus(OrderStatus.Confirmed); // Робимо підтвердженим
        store.Add(order);

        var fakeImporter = new FakeOrderImporter(new[] { Order.Create("ORD-1", "CUST-1", 2000m) });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => service.ImportAsync(fakeImporter, "dummy.json"));
        Assert.Contains("вже підтверджено", ex.Message);
        Assert.Equal(1000m, store.GetById("ORD-1")!.Total); // Сума не змінилася
    }

    // 2) Подія піднімається рівно один раз
    [Fact]
    public void Add_RaisesChangedExactlyOnce()
    {
        // Arrange
        var service = new OrderService(new InMemoryOrderStore(), new DummySink());
        var events = new List<OrderChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        // Act
        service.CreateOrder("ORD-2", "CUST-2"); // Твій метод створення

        // Assert
        Assert.Single(events);
        Assert.Equal(OrderChangeKind.Created, events[0].Kind);
    }

    // 3) Звіт на кількох наборах (Theory + InlineData)
    [Theory]
    [InlineData(1000, 2)] // 2 замовлення дорожчі за 1000
    [InlineData(5000, 0)] // 0 замовлень дорожчих за 5000
    public void ExpensiveOrders_CountsCorrectly_BasedOnThreshold(decimal threshold, int expectedCount)
    {
        var items = new[] { 
            Order.Create("O1", "C1", 1500m), 
            Order.Create("O2", "C2", 2000m),
            Order.Create("O3", "C3", 500m) 
        };
        
        var result = ReportService.ExpensiveOrders(items, threshold);
        Assert.Equal(expectedCount, result.Count);
    }

    // 4) Детермінований топ-N
    [Fact]
    public void TopByTotal_IsDeterministic()
    {
        var items = new[] { 
            Order.Create("O1", "C1", 100m), 
            Order.Create("O2", "C2", 900m),
            Order.Create("O3", "C3", 500m) 
        };

        var top2 = ReportService.TopByTotal(items, 2);

        Assert.Equal(new[] { "O2", "O3" }, top2.Select(x => x.OrderId).ToArray());
    }

    // 5) Перевірка скасування (якщо токен вже скасований)
    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Скасовуємо одразу
        
        var service = new OrderService(new InMemoryOrderStore(), new DummySink());
        
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new FakeOrderImporter(new[] { Order.Create("T-1", "C-1", 10m) }), "any.json", cts.Token));
    }

    // ==========================================
    // НОВЕ: 6) Зв'язані токени (перевірка таймауту)
    // ==========================================
    [Fact]
    public async Task ImportAsync_WithLinkedToken_ThrowsOnTimeout()
    {
        // Arrange
        var store = new InMemoryOrderStore();
        var service = new OrderService(store, new DummySink());
        
        // Передаємо багато замовлень, щоб процес зайняв час
        var bigArray = Enumerable.Range(1, 1000).Select(i => Order.Create($"O-{i}", "C-1", 10m)).ToArray();
        var fakeImporter = new FakeOrderImporter(bigArray);

        using var userSource = new CancellationTokenSource();
        using var timeoutSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(1)); // Швидкий таймаут (1 мс)
        
        // Об'єднуємо токени
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(userSource.Token, timeoutSource.Token);

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(fakeImporter, "any.json", linkedSource.Token));

        // Доводимо, що скасування викликав саме таймаут, а не токен користувача
        Assert.True(timeoutSource.IsCancellationRequested);
        Assert.False(userSource.IsCancellationRequested);
    }
}

// Допоміжний клас для тестів (щоб не читати диск)
class FakeOrderImporter(Order[] orders) : IOrderImporter
{
    public async IAsyncEnumerable<Order> LoadAsync(string path, [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var order in orders)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Yield(); // Імітуємо асинхронність
            yield return order;
        }
    }
}

// Допоміжна заглушка для логера
class DummySink : Core.Abstractions.INotificationSink { public void Notify(string m){} }