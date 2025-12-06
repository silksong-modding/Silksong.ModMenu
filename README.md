# Silksong.ModMenu

The standard menu-modification mod for Silksong. It provides a small but powerful set of features for mods to use.

This mod also adds a 'Mod Options' menu to the main Options menu by default.

The rest of this page is targeted towards mod developers. If you're not developing mods, you can stop reading now.

## Mod Options

This mod adds a 'Mods' sub-menu to the bottom of the Options menu as its main feature. All other plugins can add content to this menu as they wish.

By default, any installed plugin's [`ConfigFile`](https://docs.bepinex.dev/api/BepInEx.Configuration.ConfigFile.html) data will be used to generate an ingame menu that can modify those configuration options. At this time, only booleans, enums, and explicit value lists are supported; for more complex configuration including arbitrary integers and floats, manual menu construction is needed.

Plugins can add sub-menus to the main menu through the [`Registry`](Registry.cs) class manually, or by implementing one of the [Plugin](Plugin) interfaces that extend from [`IModMenuInterface`](Plugin/IModMenuInterface.cs) for a simplified workflow. Plugins can add the [`ModMenuIgnore`](Plugin/ModMenuIgnore.cs) attribute to opt out of automatic menu creation.

## API

The ModMenu API can be understood in three separate, hierarchical categories.

### Screens

[Screens](Screens) are individual menu screens, of which the player can only ever see one at a time. For most use cases, [`SimpleMenuScreen`](Screens/SimpleMenuScreen.cs) is all you need to support some basic configuration, but for larger, more complicated, and more dense menus, you will want to look into the alternative implemenations of [`AbstractMenuScreen`](Screens/AbstractMenuScreen.cs).

Creating a new screen object instantiates all the necessary Unity prefabs, parents them to the `UIManager` instance and leaves them inactive. To get your menu to appear, you'll need to inject an entry point using the [`Registry`](Registry.cs) from which you invoke [`MenuScreenNavigation.Show(...)`](Screens/MenuScreenNavigation.cs).  `MenuScreenNavigation` is the goto static class for navigating forwards and backwards through custom menus.

Menu screens are regenerated through the Registry APIs every time the UIManager is recreated, which can be many times throughout a play session. Care should be taken not to leak event subscribers during the menu creation process.

Developers can implement their own [`AbstractMenuScreen`](Screens/AbstractMenuScreen.cs) subclasses to define new layout algorithms if they so choose.

### Elements

[Elements](Elements) are the individual assets which populate a menu screen, such as buttons, labels, choosers, and sliders. Screens control how elements added to them are organized and how controller navigation works amongst them.

For consistent user experience, elements also define standard [Colors](Elements/Colors.cs) and [Font sizes](Elements/FontSizes.cs) to represent different states, such as true/false buttons, and invalid data.

Developers can implement their own [`MenuElement`](Elements/MenuElement.cs) subclasses to provide new mechanisms if they so choose. Just note that if they are intended to be controller-navigable, they must specifically be [`SelectableElement`](Elements/SelectableElement.cs)s too.

### Models

[Models](Model) are low-level data containers and listeners, divorced from any Unity dependencies. They are simple and extensible classes and interfaces for managing the data represented in a [`MenuElement`](Elements/MenuElement.cs).

For instance, a [`ChoiceElement`](Elements/ChoiceElement.cs) has its data modeled by a [`IChoiceModel`](Models/IChoiceModel.cs), which controls what the underlying data type is, and what happens to it when the user navigates left or right. In most cases, a [`ListChoiceModel`](Models/ListChoiceModel.cs) is sufficient, but advanced use cases may call for custom behavior, such as dynamic filtering, or infinite scroll. The low-level model interfaces support such use cases.

Models are stateful and remember which value they currently represent, so you should construct new instances for every piece of data you want to model; i.e., if your configuration has 3 different `boolean` parameters, you need 3 separate models to represent them, you should not *reuse* an existing model unless you really do want to show the same data in multiple parts of the UI.

It is recommended, though not required, that you create new Models every time your menu is regenerated (and remove old subscribers when the previous menu is disposed).

## Future work

The ModMenu mod should be considered _unstable_ version 1.0 is released. Breaking API changes may occur in the pursuit of implementing additional features to reach 1.0.

Required for 1.0:
*   New content pane implementations:
	*   Grid
*   New menu element implementations:
	*   Labels, icons
	*   Raw text input

Optional future work:
*   Extending the 'new game' menu with new game modes.
*   New content pane implementations:
	*   Multi-row
	*   Scroll pane
*   New menu element implementations:
	*   Icons/images
	*   Keybinds
