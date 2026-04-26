using Silksong.ModMenu.Elements;

namespace Silksong.ModMenu.Generator;

/// <summary>
/// Event generated whenever any menu element of an ICustomMenu has its value changed.
/// </summary>
/// <param name="MemberName">The field or property name of the data class that got changed.</param>
/// <param name="Value">The new boxed value. May be null if the field is managed by a sub-menu.</param>
public record CustomMenuValueChangedEvent(string MemberName, object? Value) { }
