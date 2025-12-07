using System;

namespace Silksong.ModMenu.Internal;

internal class EventSuppressor
{
    private int suppressors;

    public bool Suppressed => suppressors > 0;

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
