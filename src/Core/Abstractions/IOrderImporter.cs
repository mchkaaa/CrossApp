using System.Collections.Generic;
using System.Threading;
using CrossApp.Core.Domain;

namespace Core.Abstractions;

public interface IOrderImporter
{
    IAsyncEnumerable<Order> LoadAsync(string path, CancellationToken ct = default);
}