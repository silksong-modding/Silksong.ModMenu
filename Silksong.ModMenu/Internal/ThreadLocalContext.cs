using System;
using System.Diagnostics.CodeAnalysis;

namespace Silksong.ModMenu.Internal;

internal static class ThreadLocalContext<T>
    where T : class
{
    [ThreadStatic]
    private static T? currentValue;

    internal static bool Get([MaybeNullWhen(false)] out T value)
    {
        var v = currentValue;
        if (v != null)
        {
            value = v;
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    internal static IDisposable Set(T value) => new Scoped(value);

    private class Scoped : IDisposable
    {
        private readonly T? prev;

        public Scoped(T value)
        {
            prev = currentValue;
            currentValue = value;
        }

        public void Dispose() => currentValue = prev;
    }
}
