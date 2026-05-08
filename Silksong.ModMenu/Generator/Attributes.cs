using System;

namespace Silksong.ModMenu.Generator;

/// <summary>
/// Attribute to automatically generate a custom mod menu class for a given data type.
/// Can also be applied to a non-public field or property on such a class to force its inclusion.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class GenerateMenuAttribute : Attribute { }

/// <summary>
/// Attribute to give a custom field generator for the annotated field or property.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class ElementFactoryAttribute<T> : Attribute
    where T : IElementFactory, new() { }

/// <summary>
/// Attribute to apply to any non-public field or property to ensure it gets a menu element generated.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class ModMenuIncludeAttribute : Attribute { }

/// <summary>
/// Attribute to apply to any property with a finite number of acceptable values.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class ModMenuOptionsAttribute(params object[] options) : Attribute
{
    /// <summary>
    ///
    /// </summary>
    public readonly object[] Options = options;
}

/// <summary>
/// Attribute to apply to any numeric property, to specify a minimum and maximum.
/// Dynamic mins/maxes are not yet supported.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class ModMenuRangeAttribute(object Min, object Max) : Attribute
{
    /// <summary>
    /// The minimum value of this element.
    /// </summary>
    public readonly object Min = Min;

    /// <summary>
    /// The maximum value of this element.
    /// </summary>
    public readonly object Max = Max;
}

/// <summary>
/// Attribute to mark a field or property of a data class as requiring its own custom sub-menu, of the parameterized type.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class SubMenuAttribute<T> : Attribute
    where T : ICustomMenu, new() { }
