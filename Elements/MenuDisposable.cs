using System;

namespace Silksong.ModMenu.Elements;

public abstract class MenuDisposable : IDisposable
{
    public event Action? OnDispose
    {
        add
        {
            if (disposed)
                value?.Invoke();
            else
                OnDisposeInternal += value;
        }
        remove => OnDisposeInternal -= value;
    }

    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        OnDisposeInternal?.Invoke();
    }

    private bool disposed = false;
    private event Action? OnDisposeInternal;
}
