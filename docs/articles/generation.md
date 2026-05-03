# Menu Generation

Mod Menu supports automatic menu generation in two different ways.

## BepInEx ConfigFile

By default, all config entries with supported types defined in your plugin's ConfigFile will be converted into menu elements in your mod's page.

This happens for free, automatically, even if you don't add a dependency on ModMenu. You can opt out of this behaviour by adding a @"Silksong.ModMenu.Models.ModMenuIgnoreAttribute" attribute to your plugin class.

There are several ways you can modify or divert from this behaviour if the defaults are not sufficient for your use case.

### Customize specific ConfigEntries

You can change the MenuElement generated for any config entry by tagging it with a @"Silksong.ModMenu.Plugin.ConfigEntryFactory.MenuElementGenerator". ModMenu will run the delegate to generate the corresponding menu element rather than use its default mappings.

ModMenu also generally respects AcceptableValue lists, and ranges for numeric types.  Unsupported types are silently ignored.

### Organize into sub-pages

If your mod has too many config options and the paginated format does not do it justice, you might consider organizing your options into named sub-pages.

By implementing the @"Silksong.ModMenu.Plugin.IModMenuNestedMenu" interface, ModMenu will use the hierarchical path structure of your config entries (treating '.' as the separator) to generate sub-pages.

You can also instead annotate specific entries with @"Silksong.ModMenu.Plugin.ConfigEntrySubgroup" to organize them into pages independently of their path structure.

### One Big Button

If your mod's settings can be boiled down to a single big on-off button, you can implement the @"Silksong.ModMenu.Plugin.IModMenuToggle" interface to avoid the sub-menu entirely.

### Manually crafted menu

If none of the above are sufficient for your use case, you'll want to implement the @"Silksong.ModMenu.Plugin.IModMenuCustomMenu" interface and build your own menu screen from scratch using ModMenu's APIs.

If some parts of config-based generation are still useful to you, you can use the @"Silksong.ModMenu.Plugin.ConfigEntryFactory" to access those utilities, cherrypicking it for specific config entries, or you can even extend the class to override its behaviours piecemeal. See the class documentation for more information.

## Source-based Generation

If your configuration is managed through a data class rather than config entries, you may want to take advantage of the @"Silksong.ModMenu.Generator.GenerateMenuAttribute" attribute.  Annotating a class `Foo` with this attribute will generate a `FooMenu` class in the same namespace that implements the @"Silksong.ModMenu.Generator.ICustomMenu`1" interface, with named menu elements for each corresponding field or property. For example:

```
[GenerateMenu]
public class Foo
{
    public int Bar;
    public string Baz;
}
```

Will generate a class like:

```
public class FooMenu : ICustomMenu<Foo>
{
    public SelectableValueElement<int> Bar = ...;
    public SelectableValueElement<string> Baz = ...;

    ...
}
```

After instantiating your generated menu class, you can add the elements to your custom page manually by name or in aggregate using the @"Silksong.ModMenu.Generator.ICustomMenu`1.Elements" method. Menu state can be synced to and from your underlying data storage using the @"Silksong.ModMenu.Generator.ICustomMenu`1.ExportTo(`0)" and @"Silksong.ModMenu.Generator.ICustomMenu`1.ApplyFrom(`0)" methods respectively. You can listen to the menu class's @"Silksong.ModMenu.Generator.ICustomMenu`1.OnValueChanged" event to be notified whenever any menu element has its value changed, but you must explicitly call @"Silksong.ModMenu.Generator.ICustomMenu`1.ApplyFrom(`0)" yourself whenever modifying your underlaying data object through other means, as ModMenu is not aware of whatever storage mechanism you are using.

### Customizing Elements

Each declared element on your custom menu class can be changed directly, via setting the property to a new element. If you don't want a property to be managed by the custom menu, annotate it with @"Silksong.ModMenu.Models.ModMenuIgnoreAttribute" in the data class.

You can also succinctly specify minimums and maximums for numeric properties, using @"Silksong.ModMenu.Generator.ModMenuRangeAttribute". If you have a special type or you want to use a custom element for a specific property, use the @"Silksong.ModMenu.Generator.ElementFactoryAttribute`1" with an @"Silksong.ModMenu.Generator.IElementFactory`2" implementing class.

### Sub-menus

If your data class has nested data classes within it, you can create sub menus for those using @"Silksong.ModMenu.Generator.SubMenuAttribute`1". While you can use the @"Silksong.ModMenu.Generator.GenerateMenuAttribute"-generated class for these, it is not required; any type which implements @"Silksong.ModMenu.Generator.ICustomMenu`1" for the property's data type is permitted, in case you wish to provide a custom implementation.

For instance, if you have a class like:

```
[GenerateMenu]
public class Bar
{
    public int MyInt = 2;
}
```

You can include it in a containing class by:

```
[GenerateMenu]
public class Foo
{
    [SubMenu<BarMenu>]
	public Bar Bar = new();
}
```
