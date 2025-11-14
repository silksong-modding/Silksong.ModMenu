using System.Collections.Generic;

namespace Silksong.ModMenu.Internal;

internal static class CollectionsUtil
{
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
}
