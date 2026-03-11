using System;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Helper class for managing hierarchical visibility of elements.
///
/// This mirrors the GameObject visibility model, without requiring a GameObject.
/// </summary>
/// <param name="DefaultVisibility">Whether this entity should be treated as visible without a parent.</param>
public class VisibilityManager(bool DefaultVisibility)
{
    private VisibilityManager? parent;

    /// <summary>
    /// Whether this entity should be visible if its ancestors are visible.
    /// </summary>
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

    /// <summary>
    /// Whether this entity is visible in conjunction with its ancestors.
    /// </summary>
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

    /// <summary>
    /// Set the direct parent of this entity. Can only be set once.
    /// </summary>
    public void SetParent(VisibilityManager parent)
    {
        this.parent?.OnVisibilityChanged -= UpdateVisibleInHierarchy;
        this.parent = parent;
        this.parent.OnVisibilityChanged += UpdateVisibleInHierarchy;
        UpdateVisibleInHierarchy();
    }

    /// <summary>
    /// Remove this entity from its parent, make it headless.
    /// </summary>
    public void ClearParent()
    {
        parent?.OnVisibilityChanged -= UpdateVisibleInHierarchy;
        parent = null;
        UpdateVisibleInHierarchy();
    }

    private void UpdateVisibleInHierarchy(bool parentVisible) => UpdateVisibleInHierarchy();

    private void UpdateVisibleInHierarchy() =>
        VisibleInHierarchy = VisibleSelf && (parent?.VisibleInHierarchy ?? DefaultVisibility);
}
