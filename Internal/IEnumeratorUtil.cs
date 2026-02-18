using System;
using System.Collections;

namespace Silksong.ModMenu.Internal;

internal static class IEnumeratorUtil
{
    // Returns a modified iterator which provides the given ThreadLocalContext during all `MoveNext()` calls.
    internal static IEnumerator WithContext<T>(this IEnumerator self, T context)
        where T : class
    {
        IEnumerator Modified()
        {
            while (true)
            {
                bool moveNext = false;
                object? current = null;

                using (ThreadLocalContext<T>.Set(context))
                {
                    moveNext = self.MoveNext();
                    if (moveNext)
                        current = self.Current;
                }

                if (moveNext)
                    yield return current;
                else
                    yield break;
            }
        }

        return Modified();
    }

    internal static IEnumerator PropagateContext<T>(this IEnumerator self)
        where T : class => ThreadLocalContext<T>.Get(out var ctx) ? self.WithContext<T>(ctx) : self;

    internal static IEnumerator Append(this IEnumerator self, Action action)
    {
        IEnumerator Modified()
        {
            while (self.MoveNext())
                yield return self.Current;
            action();
        }

        return Modified();
    }
}
