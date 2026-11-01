using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossApp.Core.Reports;

public static class EnumerableExtensions
{
    // Власний метод-розширення, що працює для будь-якого типу T та ключа сортування
    public static IEnumerable<T> TopBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, int n)
    {
        return source.OrderByDescending(keySelector).Take(n);
    }
}