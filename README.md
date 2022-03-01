# XUiComponents

A 7 Days to Die modder's resource that adds some new XUi Widgets and XUi Controllers that modders can use

## Features

### New Widgets

* `curvedlabel` a curved text labels around a central point
* `scrollview` a scrollable panel that can have other vwidgets inside
* `scrollbar` a scrollbar that goes with the scrollview

### New Controllers

* `XUiC.Spinner` spins the view that it is attached to
* `XUiC.DialIndicator` makes the view that it is attached to act like a dial indicator, useful to make a speedometer or
fuel tank gauge
* `XUiC.ItemStack` an updated `ItemStack` controller that includes some additional bindings and features
* `XUiC.Backpack` an updated `Backpack` controller that includes new features like search
* `XUiC.LootContainer` an updated `LootContainer` controller that includes new features like search
* `XUiC.VehicleContainter` an updated `VehicleContainter` controller that includes new features like search
* `XUiC.MapInvitesListEntry` an updated `MapInvitesListEntry` controller that includes new bindings
* `XUiC.MapWaypointListEntry` an updated `MapWaypointListEntry` controller that includes new bindings

### Changes to Vanilla Widgets and Controllers

* Allows `mapViewTexture` to be resized to different aspect ratios, no more 1:1 aspect ratios for the map

### UI Debugging

* Adds a console command `xuic debug` that enables some debugging information and logging to the console

## Include in your mod

There are two ways to include this mod in your mod.

First you have to download one of the [releases](https://github.com/s7092910/XUiComponents/releases/).
In the download, there are a total of 5 files in the XUiComponents folder, the structure of the XUiComponents
folder should look as followed:

```text
Config
    - XUi_Common
        - styles.xml
License
    - LICENSE.md

Modinfo.xml
XUiComponents.dll
XUiComponentsLoader.dll
```

Please be aware of the [License](LICENSE.md) for use of XUiComponents

### As a seperate mod

To add the mod as a seperate mod, extract the XUiComponents folder and add it to the
7 Days to Die Mods folder. From there, 7 Days to Die will load it as a seperate mod which will allow
access the all the new widgets and controllers from any mod in a seperate mod folder.

Make sure that the XUiComponents folder is included in the download of the mod or include
instructions for users of the mod on how to download and install the XUiComponents standalone mod.

### Include in an existing mod's folder

To add the mod as part of an existing mod, extract all the files from the XUiComponents folder except
for the Modinfo.xml into the mod's folder. 7 Days to Die will load the XUiComponentsLoader.dll from
the mod's folder when the game is started.

When you bundle up the mod, make sure that all the files from the XUiComponents folder except
for the Modinfo.xml are in the mod's folder. Mod users will not need to download any additional files.

#### If your mod includes its own dll with a class that extends `IModApi`

As XUiComponents uses Harmony to patch some of the 7 Days to Die code to work, it requires to be patched by Harmoney.
XUiComponentsLoader.dll does the Harmony patching, but uses `IModApi` to do so. One of the limitations of this is that
a mod folder can only have one dll that extends `IModApi`, so in the class that extends `IModApi`, a call can be used to patch the XUiComponents.dll.

To start, add XUiComponents.dll as a reference to the mod's source code project. Then in the class extending `IModApi` add the following lines. You may pass in your own Harmony object if you have one already.

```C#
    Harmony harmony = new Harmony(GetType().ToString());
    XUiComponents.LoadXuiComponents(harmony);
```

#### If your mod includes it own XUi_Common styles.xml

Add the following lines to the mod's styles.xml in the XUi_Common folder

```xml
 <append xpath="/styles">
    <style type="curvedlabel">
        <style_entry name="color" value="[labelColor]"/>
        <style_entry name="font_face" value="ReferenceFont"/>
        <style_entry name="font_size" value="28"/>
        <style_entry name="justify" value="left"/>
        <style_entry name="effect" value="none"/>
        <style_entry name="pivot" value="topleft"/>
        <style_entry name="text" value=""/>
        <style_entry name="upper_case" value="false" />
    </style>
 </append>
```

#### Additonal Steps

After picking one of the two options above, there are a few additional steps that must be taken. As
the PlayerStatController adds additional code to 7 Days to Die, Easy Anti Cheat must be turned off on the client
if someone is using a mod that includes the XUiComponents.

[How to turn off East Anti Cheat on 7 Days to Die](https://www.youtube.com/watch?v=752cb_A9Leg)

#### Caveats

For a mod using the XUiComponents to work on multiplayer, the mod must also be installed on the
server and the server must have Easy Anti Cheat turned off.

## Usage

[Using the New Controllers](tutorials/Controllers.md)

[Using the New Widgets](tutorials/Widgets.md)

[Using the new changes to vanilla Widgets](tutorials/VanillaWidgets.md)

[Using the XUiC Console/Debugging mode](tutorials/XUiC-Console.md)

## Changelog

[Changelog](CHANGELOG.md)

## Credits/The Team

* [Christopher Beda](https://github.com/s7092910) - Author
* Sirillion - Ideas, feeback and testing

## Mods using XUiComponents

* [SMX By Sirillion](https://www.nexusmods.com/7daystodie/mods/22)

If you have a mod that is using XUiComponents and want to get it added to this list, create an issue and it will be added
to this list shortly.

## License

```Text
    Copyright 2022 Christopher Beda

    You may not use this file except in compliance with the License.
    You may obtain a copy of the License at

    https://github.com/s7092910/XUiComponents/blob/main/LICENSE.md

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
```
