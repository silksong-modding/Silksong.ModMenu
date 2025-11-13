using System;

namespace Silksong.ModMenu.Models;

/// <summary>
/// Provides a custom name for any field or enum value represented through the Models helper.
/// </summary>
/// <param name="name"></param>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class ModMenuName(string customName) : Attribute
{
    public string CustomName => customName;
}

/// <summary>
/// Indicate that this field, property, or enum value should be ignored.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class ModMenuIgnore : Attribute { }
