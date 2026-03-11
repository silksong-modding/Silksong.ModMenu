using System;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// The base class for all menu screen resources. Allows actions to be taken on UI destruction.
/// </summary>
public abstract class MenuDisposable : IDisposable
{
    /// <summary>
    /// Invoked when this menu element is disposed, once. If already disposed, subscriptions are invoked instantly.
    /// </summary>
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

    /// <inheritdoc/>
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
