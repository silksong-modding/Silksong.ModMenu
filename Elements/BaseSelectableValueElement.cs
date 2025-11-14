using Silksong.ModMenu.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Base class for all typed SelectableValueElements, for erasure.
/// </summary>
public abstract class BaseSelectableValueElement(GameObject container, Selectable selectable)
    : SelectableElement(container, selectable)
{
    /// <summary>
    /// Get the type-erased version of the value model beneath.
    /// </summary>
    public abstract IBaseValueModel RawModel { get; }
}
