using System;

namespace Silksong.ModMenu.Util;

/// <summary>
/// A safe ref-counting utility intended for suppressing infinite event cascade when updating UI elements.
/// </summary>
public class EventSuppressor
{
    private int suppressors;

    /// <summary>
    /// Returns true if any suppressor for this event exists.
    /// </summary>
    public bool Suppressed => suppressors > 0;

    /// <summary>
    /// Suppresses this event for the duration of the returned disposable.
    /// </summary>
    public IDisposable Suppress() => new Lease(this);

    private class Lease : IDisposable
    {
        private readonly EventSuppressor parent;

        public Lease(EventSuppressor parent)
        {
            this.parent = parent;
            parent.suppressors++;
        }

        public void Dispose() => parent.suppressors--;
    }
}
