using Silksong.ModMenu.Elements;
using UnityEngine;

namespace Silksong.ModMenu.Generator;

/// <summary>
/// Generator for a <see cref="Color"/> element which only accepts RGB values with full alpha.
/// </summary>
public class RGBElementFactory : IElementFactory<Color, ColorInput>
{
    /// <inheritdoc/>
    public ColorInput CreateElement(LocalizedText name, LocalizedText description) =>
        new(name, description) { Format = ColorInput.InputFormat.RGB };
}
