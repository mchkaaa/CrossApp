using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using CrossApp.Core.Domain;
using CrossApp.Core.Dto;
using Core.Abstractions;

namespace Core.Storage;

public class JsonOrderImporter : IOrderImporter
{
    public async IAsyncEnumerable<Order> LoadAsync(string path, [EnumeratorCancellation] CancellationToken ct = default)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        
        // Читаємо потоково по одному замовленню
        var dtos = JsonSerializer.DeserializeAsyncEnumerable<OrderDto>(stream, cancellationToken: ct);
        
        await foreach (var dto in dtos)
        {
            if (dto != null)
                yield return Order.FromDto(dto);
        }
    }
}