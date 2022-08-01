# Quartz

A 7 Days to Die modder's resource that adds some new XUi Widgets and XUi Controllers that modders can use.

## Important Announcements

XUiComponents has been rebranded as Quartz. There is a [guide](tutorials/UpgradingFromXUiComponents.md) to help
upgrading from XUiComponents to Quartz.

## Features

### New Widgets

* `curvedlabel` a curved text labels around a central point
* `scrollview` a scrollable panel that can have other vwidgets inside
* `scrollbar` a scrollbar that goes with the scrollview

### New Controllers

* `Quartz.Spinner` spins the view that it is attached to
* `Quartz.DialIndicator` makes the view that it is attached to act like a dial indicator, useful to make a speedometer or
fuel tank gauge
* `Quartz.ItemStack` an updated `ItemStack` controller that includes some additional bindings and features
* `Quartz.Backpack` an updated `Backpack` controller that includes new features like search
* `Quartz.LootContainer` an updated `LootContainer` controller that includes new features like search
* `Quartz.VehicleContainer` an updated `VehicleContainer` controller that includes new features like search
* `Quartz.MapInvitesListEntry` an updated `MapInvitesListEntry` controller that includes new bindings
* `Quartz.MapWaypointListEntry` an updated `MapWaypointListEntry` controller that includes new bindings
* `Quartz.RandomTexture` allows textures to be randomly selected to be displayed when the window holding the element with the controller is opened

### Changes to Vanilla Widgets and Controllers

* Allows `mapViewTexture` to be resized to different aspect ratios, no more 1:1 aspect ratios for the map

### UI Debugging

* Adds a console command `quartz debug` that enables some debugging information and logging to the console

## Include in your mod

There are two ways to include this mod in your mod.

First you have to download one of the [releases](https://github.com/s7092910/Quartz/releases/).
In the download, there are a total of 5 files in the Quartz folder, the structure of the Quartz
folder should look as followed:

```text
Config
    - XUi_Common
        - styles.xml
License
    - LICENSE.md

Modinfo.xml
Quartz.dll
QuartzLoader.dll
```

Please be aware of the [License](LICENSE.md) for use of Quartz

### As a seperate mod

To add the mod as a seperate mod, extract the Quartz folder and add it to the
7 Days to Die Mods folder. From there, 7 Days to Die will load it as a seperate mod which will allow
access the all the new widgets and controllers from any mod in a seperate mod folder.

Make sure that the Quartz folder is included in the download of the mod or include
instructions for users of the mod on how to download and install the Quartz standalone mod.

### Include in an existing mod's folder

To add the mod as part of an existing mod, extract all the files from the Quartz folder except
for the Modinfo.xml into the mod's folder. 7 Days to Die will load the QuartzLoader.dll from
the mod's folder when the game is started.

When you bundle up the mod, make sure that all the files from the Quartz folder except
for the Modinfo.xml are in the mod's folder. Mod users will not need to download any additional files.

#### If your mod includes its own dll with a class that extends `IModApi`

As Quartz uses Harmony to patch some of the 7 Days to Die code to work, it requires to be patched by Harmoney.
QuartzLoader.dll does the Harmony patching, but uses `IModApi` to do so. One of the limitations of this is that
a mod folder can only have one dll that extends `IModApi`, so in the class that extends `IModApi`, a call can be used to patch the Quartz.dll.

To start, add Quartz.dll as a reference to the mod's source code project. Then in the class extending `IModApi` add the following lines. You may pass in your own Harmony object if you have one already.

```C#
    Harmony harmony = new Harmony(GetType().ToString());
    QuartzMod.LoadQuartz(harmony);
```

#### If your mod includes it own XUi_Common and/or XUi files

Look in the XUi_Common or XUi Folder and copy and paste the lines from each file to their respective in each folder in your
Mod's config folder.

#### Additonal Steps

After picking one of the two options above, there are a few additional steps that must be taken. As
the PlayerStatController adds additional code to 7 Days to Die, Easy Anti Cheat must be turned off on the client
if someone is using a mod that includes the Quartz.

[How to turn off East Anti Cheat on 7 Days to Die](https://www.youtube.com/watch?v=752cb_A9Leg)

#### Caveats

For a mod using the Quartz to work on multiplayer, the mod must also be installed on the
server and the server must have Easy Anti Cheat turned off.

## Usage

[Using the New Controllers](tutorials/Controllers.md)

[Using the New Widgets](tutorials/Widgets.md)

[Using the new changes to vanilla Widgets](tutorials/VanillaWidgets.md)

[Using the Quartz Console/Debugging mode](tutorials/Quartz-Console.md)

## Changelog

[Changelog](CHANGELOG.md)

## Credits/The Team

* [Christopher Beda](https://github.com/s7092910) - Author
* Sirillion - Ideas, feeback and testing

## Mods using Quartz

* [SMX By Sirillion](https://www.nexusmods.com/7daystodie/mods/22)

If you have a mod that is using Quartz and want to get it added to this list, create an issue and it will be added
to this list shortly.

## Contact
There is a hosted channel in Guppy's Unofficial 7DtD Modding Server Discord. The channel is `#laydors-toolshed`

* [Guppy's Unofficial 7DtD Modding Server Discord](https://discord.gg/mQpvj95rvW)

## License

```Text
    Copyright 2022 Christopher Beda

    You may not use this file except in compliance with the License.
    You may obtain a copy of the License at

    https://github.com/s7092910/Quartz/blob/main/LICENSE.md

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
```
