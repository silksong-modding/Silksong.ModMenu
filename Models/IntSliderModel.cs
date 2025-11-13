namespace Silksong.ModMenu.Models;

/// <summary>
/// A simple SliderModel which maps the index directly.
/// </summary>
public class IntSliderModel(int min, int max) : SliderModel<int>(min, max)
{
    /// <inheritdoc/>
    protected override bool GetIndex(int value, out int index)
    {
        index = value;
        return value >= MinimumIndex && value <= MaximumIndex;
    }

    /// <inheritdoc/>
    protected override int GetValue(int index) => index;
}
