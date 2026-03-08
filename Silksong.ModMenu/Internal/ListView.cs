using System;
using System.Collections;
using System.Collections.Generic;

namespace Silksong.ModMenu.Internal;

// Convenience class for random-access of peculiarly organized data.
internal class ListView<T>(Func<int, T> getter, int count) : IReadOnlyList<T>
{
    public T this[int index] => getter(index);

    public int Count => count;

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
