namespace Silksong.ModMenu.Models;

/// <summary>
/// A set of text values that can be navigated with left and right inputs.
/// This provides the minimal functionality necessary to power a UI element.
/// </summary>
public interface IBaseChoiceModel : IDisplayable
{
    /// <summary>
    /// Move to the 'previous' value is this model's domain.
    /// </summary>
    /// <returns>True if the value was changed, false if not.</returns>
    bool MoveLeft();

    /// <summary>
    /// Move to the 'next' value in this model's domain.
    /// </summary>
    /// <returns>True if the value was changed, false if not.</returns>
    bool MoveRight();
}
