using System;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Helper class for managing hierarchical visibility of elements.
///
/// This mirrors the GameObject visibility model, without requiring a GameObject.
/// </summary>
public class VisibilityManager
{
    private VisibilityManager? parent;

    public bool VisibleSelf
    {
        get => field;
        set
        {
            if (field == value)
                return;

            field = value;
            UpdateVisibleInHierarchy();
        }
    } = true;

    public bool VisibleInHierarchy
    {
        get => field;
        private set
        {
            if (field == value)
                return;

            field = value;
            OnVisibilityChanged?.Invoke(value);
        }
    } = true;

    /// <summary>
    /// Event notified when VisibileInHierarchy changes.
    /// </summary>
    public event Action<bool>? OnVisibilityChanged;

    public void SetParent(VisibilityManager parent)
    {
        if (this.parent != null)
            throw new ArgumentException($"{nameof(parent)} akready set");

        this.parent = parent;
        parent.OnVisibilityChanged += _ => UpdateVisibleInHierarchy();
        UpdateVisibleInHierarchy();
    }

    private void UpdateVisibleInHierarchy() =>
        VisibleInHierarchy = VisibleSelf && (parent?.VisibleInHierarchy ?? true);
}
