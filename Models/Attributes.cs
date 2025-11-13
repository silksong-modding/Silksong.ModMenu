using System;
using System.Linq;
using System.Reflection;

namespace Silksong.ModMenu.Models;

/// <summary>
/// Provides a custom name for any enum value represented through the Models helper.
/// </summary>
/// <param name="name"></param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ModMenuName(string customName) : Attribute
{
    public string CustomName => customName;
}

/// <summary>
/// Indicate that this plugin class or enum value should be ignored.
///
/// Any attribute named "ModMenuIgnore" will be treated the same for this functionality, so you can opt-out of ModMenu behaviors without adding ModMenu as a dependency.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false)]
public class ModMenuIgnore : Attribute { }

internal static class AttributeExtensions
{
    internal static bool IgnoreForModMenu(this MemberInfo self) =>
        self.GetCustomAttribute<ModMenuIgnore>() != null
        || self.GetCustomAttributes(true).Any(attr => attr.GetType().Name == nameof(ModMenuIgnore));
}
