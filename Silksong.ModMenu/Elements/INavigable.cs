using System.Collections;
using System.Collections.Generic;
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
    /// Set the directional neighbors of this entity to one or more of the given choices.
    /// </summary>
    /// <returns>False if this entity has no navigation to connect.</returns>
    void SetNeighbors(NavigationDirection direction, IEnumerable<Selectable> selectables);

    /// <summary>
    /// Unset the given directional neighbor of this entity.
    /// </summary>
    void ClearNeighbors(NavigationDirection direction);

    /// <summary>
    /// Get a set of choices for the Selectables to target if navigating to this element along 'direction'.
    ///
    /// In other words, typical usage would entail:
    ///   if (foo.GetSelectables(dir, out var selectables))
    ///       bar.SetNeighbors(dir, selectables);
    /// </summary>
    bool GetSelectables(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out IEnumerable<Selectable> selectables
    );
}
