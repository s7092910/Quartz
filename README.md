![](Images/Q_logo_subtle_shadow.png)

A 7 Days to Die modder's resource that adds some new XUi Widgets and XUi Controllers that modders can use.

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

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
```
