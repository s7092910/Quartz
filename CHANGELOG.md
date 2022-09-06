# Changelog
All notable changes to this project will be documented in this file.

This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.0] - 2022-02-03

### New Bindings Added

- `{PlayerHealth+Max}`
- `{PlayerHealth+WithMax}`
- `{PlayerHealth+Percentage}`
- `{PlayerStamina+Max}`
- `{PlayerStamina+WithMax}`
- `{PlayerStamina+Percentage}`
- `{PlayerFood+Max}`
- `{PlayerFood+WithMax}`
- `{PlayerFood+Percentage}`
- `{PlayerWater+Max}`
- `{PlayerWater+WithMax}`
- `{PlayerWater+Percentage}`
- `{PlayerBagUsedSlots}`
- `{PlayerBagSize}`
- `{PlayerCarryCapacity}`
- `{PlayerMaxCarryCapacity}`
- `{InventoryIsFlashLightOn}`
- `{InventoryIsHandFlashLightOn}`
- `{InventoryIsGunFlashLightOn}`
- `{InventoryIsHelmetFlashLightOn}`

### Added

- Bindings are no longer case sensitive.
- BindingAlias created, it allows mod creators to use a single class for data bindings that are heavily related
like stats related to character health.
- Vehicle base bindings to make it easier for mod creators to access bindings related to EntityVehicle objects that
are attached to the player
- Inform added to Logging so that certain information is still logged if logging is turned off.
- Bindings will be logged to the console if they are found.
- FormatUtil class created for the formatting of strings

### Changed

- Changed BindingType to Binding
- Moved static logic out of BindingType to a static class Bindings
- Binding `{PlayerHealthCurrent}` changed to `{PlayerHealth}`
- Binding `{PlayerStaminaCurrent}` changed to `{PlayerStamina}`
- Binding `{PlayerFoodCurrent}` changed to `{PlayerFood}`
- Binding `{PlayerWaterCurrent}` changed to `{PlayerWater}`

### Removed

- CharacterMainStat, it has been replaced by BindingAlias.

[unreleased]: https://github.com/s7092910/PlayerStatController/compare/v2.0.0...HEAD
[2.0.0]: https://github.com/s7092910/PlayerStatController/compare/v1.0.0...v2.0.0
[1.0.0]: https://github.com/s7092910/PlayerStatController/compare/v1.0.0-RC2...v1.0.0
[1.0.0-RC2]: https://github.com/s7092910/PlayerStatController/releases/tag/v1.0.0-RC2