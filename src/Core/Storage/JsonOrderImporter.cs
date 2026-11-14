using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CrossApp.Core.Domain;
using CrossApp.Core.Dto;
using Core.Abstractions;
using System.Text.Encodings.Web; // ДОДАНО: Для налаштувань енкодера

namespace Core.Storage;

public class JsonOrderImporter : IOrderImporter
{
    // ДОДАНО: Налаштування серіалізатора за вимогами Лабораторної 9
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Читає ключі типу customerId замість CustomerId
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Правильно обробляє кирилицю
        PropertyNameCaseInsensitive = true
    };

    public async IAsyncEnumerable<Order> LoadAsync(string path, [EnumeratorCancellation] CancellationToken ct = default)
    {
        // Відкриваємо потік із useAsync: true для справжньої асинхронності
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        
        // Читаємо потоково по одному замовленню, передаємо наші налаштування Options
        var dtos = JsonSerializer.DeserializeAsyncEnumerable<OrderDto>(stream, Options, cancellationToken: ct);
        
        await foreach (var dto in dtos)
        {
            
            // await Task.Delay(500, ct); 

            // Перевіряємо, чи не було скасування
            ct.ThrowIfCancellationRequested();
            
            if (dto != null)
                yield return Order.FromDto(dto);
        }
    }
}