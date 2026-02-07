using Silksong.ModMenu.Elements;

namespace Silksong.ModMenu.Models;

/// <summary>
/// Base interface for all models which ultimately display a string to represent the chosen value.
/// </summary>
public interface IDisplayable
{
    /// <summary>
    /// The UI string to display for this entity.
    /// </summary>
    LocalizedText DisplayString();
}
