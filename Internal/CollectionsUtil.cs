using System;
using System.Collections.Generic;

namespace Silksong.ModMenu.Internal;

internal static class CollectionsUtil
{
    /// <summary>
    /// Sort a list by distance from the median element.
    /// </summary>
    internal static IEnumerable<T> MedianOutwards<T>(this IEnumerable<T> self)
    {
        if (self is not IReadOnlyList<T> list)
            list = [.. self];
        if (list.Count == 0)
            yield break;

        int mid = list.Count / 2;
        int low = mid - 1;
        int high = mid + 1;
        while (low >= 0 || high < list.Count)
        {
            if (low >= 0)
            {
                yield return list[low];
                low--;
            }
            if (high < list.Count)
            {
                yield return list[high];
                high++;
            }
        }
    }

    private static IEnumerable<(T, T)> Pairs<T>(this IEnumerable<T> self, bool circular)
    {
        var iter = self.GetEnumerator();
        if (!iter.MoveNext())
            yield break;

        var first = iter.Current;
        bool any = false;

        var prev = iter.Current;
        while (iter.MoveNext())
        {
            var next = iter.Current;
            yield return (prev, next);
            prev = next;
            any = true;
        }

        if (any && circular)
            yield return (prev, first);
    }

    internal static IEnumerable<(T, T)> Pairs<T>(this IEnumerable<T> self) => self.Pairs(false);

    internal static IEnumerable<(T, T)> CircularPairs<T>(this IEnumerable<T> self) =>
        self.Pairs(true);

    internal static IEnumerable<List<T>> Partition<T>(this IEnumerable<T> self, int size)
    {
        if (size < 1)
            throw new ArgumentException($"{nameof(size)} ({size}) < 1");

        var iter = self.GetEnumerator();
        while (true)
        {
            List<T> next = [];
            for (int i = 0; i < size && iter.MoveNext(); i++)
                next.Add(iter.Current);

            if (next.Count > 0)
                yield return next;
            if (next.Count < size)
                yield break;
        }
    }
}
