using BepInEx.Configuration;
using UnityEngine;

namespace Silksong.ModMenu.Plugin;

/// <summary>
/// Clamps a <see cref="Color"/> setting to always be at 100% opacity.
/// </summary>
/// <remarks>
/// Automatically generated <see cref="Elements.ColorInput"/>s for a setting with this restriction
/// will automatically have their format set to <see cref="Elements.ColorInput.InputFormat.RGB"/>.
/// </remarks>
public class RGBColorValues() : AcceptableValueBase(typeof(Color))
{
    /// <inheritdoc/>
    public override object Clamp(object value) => (value is Color c ? c : default) with { a = 1 };

    /// <inheritdoc/>
    public override bool IsValid(object value) => value is Color c && c.a == 1;

    /// <inheritdoc/>
    public override string ToDescriptionString() =>
        $"# Acceptable values: color codes from 000000FF to FFFFFFFF";
}
