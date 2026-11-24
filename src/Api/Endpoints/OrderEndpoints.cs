using System.Collections.Concurrent;
using System.Security.Cryptography;
using Api.Contracts;
using Api.Validation;
using Core.Abstractions;
using CrossApp.Core;
using CrossApp.Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class OrderEndpoints
{
    private static readonly ConcurrentDictionary<string, List<OrderLineDto>> OrderLinesCache =
        new(StringComparer.OrdinalIgnoreCase);

    public static void MapOrders(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        // 1. GET списку з пагінацією та пошуком: ?page=1&pageSize=20&search=... -> { items, total, page, pageSize }
        group.MapGet("/", (
            IOrderStore store,
            int page = 1,
            int pageSize = 20,
            string? search = null,
            string? customerId = null) =>
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            IEnumerable<Order> query = store.List();

            if (!string.IsNullOrWhiteSpace(customerId))
            {
                var cid = customerId.Trim();
                query = query.Where(o => o.CustomerId.Equals(cid, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(o =>
                    o.Id.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    o.CustomerId.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    o.Status.ToString().Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var filtered = query.ToList();
            var total = filtered.Count;

            var items = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => o.ToApiDto(OrderLinesCache.GetValueOrDefault(o.Id)))
                .ToList();

            return TypedResults.Ok(new PagedResult<OrderResponseDto>(items, total, page, pageSize));
        });

        // 2. GET /{id}
        group.MapGet("/{id}", Results<Ok<OrderResponseDto>, NotFound> (string id, IOrderStore store) =>
        {
            var found = store.GetById(id);
            return found is not null
                ? TypedResults.Ok(found.ToApiDto(OrderLinesCache.GetValueOrDefault(found.Id)))
                : TypedResults.NotFound();
        });

        // Додатковий маршрут для домену замовлень: GET /api/orders/{id}/total
        group.MapGet("/{id}/total", Results<Ok<object>, NotFound> (string id, IOrderStore store) =>
        {
            var found = store.GetById(id);
            if (found is null)
                return TypedResults.NotFound();

            return TypedResults.Ok<object>(new { id = found.Id, total = found.Total });
        });

        // 3. POST (валідація виконується автоматично через AddEndpointFilter)
        group.MapPost("/", Results<Created<OrderResponseDto>, ValidationProblem> (
            SaveOrderRequest request, IOrderStore store) =>
        {
            var id = "ORD-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(3)).ToUpperInvariant();
            var total = request.CalculateTotal();

            var created = Order.Create(id, request.CustomerId.Trim(), total);
            created.IsActive = request.IsActive;

            var lines = request.Lines!
                .Select(l => new OrderLineDto(l.ProductId.Trim(), l.Price, l.Quantity))
                .ToList();

            store.Add(created);
            OrderLinesCache[created.Id] = lines;

            return TypedResults.Created($"/api/orders/{created.Id}", created.ToApiDto(lines));
        })
        .AddEndpointFilter<ValidationFilter<SaveOrderRequest>>();

        // 4. PUT /{id} (валідація виконується автоматично через AddEndpointFilter)
        group.MapPut("/{id}", Results<Ok<OrderResponseDto>, NotFound, ValidationProblem> (
            string id, SaveOrderRequest request, IOrderStore store) =>
        {
            var existing = store.GetById(id);
            if (existing is null)
                return TypedResults.NotFound();

            var total = request.CalculateTotal();
            var replacement = Order.Create(existing.Id, request.CustomerId.Trim(), total);
            replacement.IsActive = request.IsActive;

            var lines = request.Lines!
                .Select(l => new OrderLineDto(l.ProductId.Trim(), l.Price, l.Quantity))
                .ToList();

            store.Update(replacement);
            OrderLinesCache[replacement.Id] = lines;

            return TypedResults.Ok(replacement.ToApiDto(lines));
        })
        .AddEndpointFilter<ValidationFilter<SaveOrderRequest>>();

        // Додатково: підтвердження замовлення (демонстрація мапінгу DomainException -> 409 Conflict)
        group.MapPost("/{id}/confirm", Results<Ok<OrderResponseDto>, NotFound> (string id, IOrderStore store) =>
        {
            var existing = store.GetById(id);
            if (existing is null)
                return TypedResults.NotFound();

            if (existing.Status != OrderStatus.Draft)
                throw new DomainException($"Неможливо підтвердити замовлення {id} у статусі {existing.Status}.");

            existing.ChangeStatus(OrderStatus.Confirmed);
            store.Update(existing);

            return TypedResults.Ok(existing.ToApiDto(OrderLinesCache.GetValueOrDefault(existing.Id)));
        });

        // 5. DELETE /{id}
        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, IOrderStore store) =>
        {
            var removed = store.Remove(id);
            if (!removed)
                return TypedResults.NotFound();

            OrderLinesCache.TryRemove(id, out _);
            return TypedResults.NoContent();
        });
    }
}