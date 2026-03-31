# ModMenu

[![NuGet Version](https://img.shields.io/nuget/v/Silksong.ModMenu)](https://www.nuget.org/packages/Silksong.ModMenu)

ModMenu adds a "Mods" menu to the options menu screen, and provides an API for mods to customize
their menu.

## Usage

Add the following line to your .csproj:
```
<PackageReference Include="Silksong.ModMenu" Version="0.4.5" />
```
The most up to date version number can be retrieved from [Nuget](https://www.nuget.org/packages/Silksong.ModMenu).

You will also need to add a dependency to your thunderstore.toml:
```
silksong_modding-ModMenu = "0.4.5"
```
The version number does not matter hugely, but the most up to date number can be retrieved from
[Thunderstore](https://thunderstore.io/c/hollow-knight-silksong/p/silksong_modding/ModMenu/).
If manually uploading, instead copy the dependency string from the Thunderstore link.

ModMenu should be added as a BepInEx dependency by putting the following attribute
onto your plugin class, below the BepInAutoPlugin attribute.
```
[BepInDependency(Silksong.ModMenu.ModMenuPlugin.Id)]
```
