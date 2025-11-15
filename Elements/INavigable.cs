using System.Diagnostics.CodeAnalysis;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A menu entity that can be selected and navigated to/from.
/// </summary>
public interface INavigable
{
    /// <summary>
    /// Reset all external navigation links for this entity.
    /// </summary>
    void ClearNeighbors();

    /// <summary>
    /// Set the directional neighbor of this entity.
    /// </summary>
    /// <returns>False if this entity has no navigation to connect.</returns>
    void SetNeighbor(NavigationDirection direction, Selectable selectable);

    /// <summary>
    /// Unset the given directional neighbor of this entity.
    /// </summary>
    void ClearNeighbor(NavigationDirection direction);

    /// <summary>
    /// Get the Selectable to target if navigating to this element along 'direction'.
    ///
    /// In other words, typical usage would entail:
    ///   if (foo.GetSelectable(dir, out var selectable))
    ///       bar.SetNeighbor(dir, selectable);
    /// </summary>
    bool GetSelectable(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out Selectable selectable
    );
}
