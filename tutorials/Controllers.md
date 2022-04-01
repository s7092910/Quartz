# Using the new Controllers in your mod

Once Quartz is added to your mod as shown in the [README.md](../README.md),
you can start using the new controllers in your mod.

## The Controllers

* [`Quartz.Spinner`](#quartzspinner) spins the widget that it is attached to
* [`Quartz.DialIndicator`](#quartzdialindicator) makes the widget that it is attached to act like a dial indicator, useful to make a speedometer or
fuel tank gauge
* [`Quartz.ItemStack`](#quartzitemstack) an updated `ItemStack` controller that includes some additional bindings and features
* [`Quartz.Backpack`](#quartzbackpack) an updated `Backpack` controller that includes new features like search
* [`Quartz.LootContainer`](#quartzlootcontainer) an updated `LootContainer` controller that includes new features like search
* [`Quartz.VehicleContainer`](#quartzvehiclecontainer) an updated `VehicleContainer` controller that includes new features like search
* [`Quartz.MapInvitesListEntry`](#quartzmapinviteslistentry) an updated `MapInvitesListEntry` controller that includes new bindings
* [`Quartz.MapWaypointListEntry`](#quartzmapwaypointlistentry) an updated `MapWaypointListEntry` controller that includes new bindings
* [`Quartz.RandomTexture`](#quartzrandomtexture) allows textures to be randomly selected to be displayed when the window holding the element with the controller is opened

## Add the controller in XML

In the widget that will use the controller, add the following line

```XML
<Widget
    ...
    controller="Quartz.{Name of the controller}, Quartz"
    ...
/>
```

### Example

For example, to use `Quartz.Spinner`, the xml will look like the following

```XML
<Widget
    ...
    controller="Quartz.Spinner, Quartz"
    ...
/>
```

Some controllers have additional attributes that is added to the widget that has the controller attached

## Quartz.Spinner

<p align="center" width="100%">
    <img width="33%" src="../images/Spinner.gif"> <br>
    The Spinner being used in SMXhud to show the player having an available skill
    point
</p>

### Using the Spinner Controller

The controller is added to the widget that is to be spun by the controller

```XML
<Widget
    ...
    controller="Quartz.Spinner, Quartz"
    
    spin="true|false" - True if the Spinner should be actively spinning

    angle_per_second="float" - The how fast of an angle in degrees the widget should 
            spin per second. A positive number spins the widget in a counter clockwise 
            direction, while a negative number spins the widget in a clockwise direction
    ...
/>
```

## Quartz.DialIndicator

<p align="center" width="100%">
    <img width="33%" src="../images/Dial-Indicator.gif"> <br>
    The Dial Indicator being used in SMXui as a fuel gauge in the vehicle screen
</p>

### Using the Dial Indicator Controller

The controller is added to the widget that is to be moved by the controller. The controller will handle the animation
and calculating where the indicator should be based on its current value and min and max range and their angles.

```XML
<Widget
    ...
    controller="Quartz.DialIndicator, Quartz"

    indicator_value="float" - The current value of the indicator

    start_angle="float" - The angle in degrees of the dial for when the indicator value is equal to the range_min

    end_angle="float" - The angle in degrees of the dial for when the indicator value is equal to the range_max

    range_min="float" - The minimum value represented by the indicator

    range_max="float"  - The maximum value represented by the indicator

    limit_indicator_to_range="true|false" - if the indicator is to be bound to the min and max range values. If true 
        the indicator will not move beyond the start and end angle if the indicator value is outside those ranges 

    animation_duration="float" - An estimate of how long the animation should take from changing from a previous value to
        a new value. If the indicator value has changed during the animation, the animation time will reset with the
        animation starting from the position it was in before the new indicator value.

    ...
/>
```

Notes:

* The angles here are using Unity's rotation system.
* To have the indicator to go from right to left, add negatives to the range values and the indicator value

## Quartz.ItemStack

This controller is an extension of the vanilla `ItemStack` controller. Some of the features of the `Quartz.ItemStack`
requires the use of the `Quartz.Backpack`, `Quartz.LootContainer`, or `Quartz.VehicleContainer` controllers. Those controllers
need to be attached as a controller to a parent widget of the widget containing the `Quartz.ItemStack` controller.

The `Quartz.ItemStack` controller when used in conjuction of the controllers mentioned above, are as followed:

* Showing if the `ItemStack` is locked when using the inventory locking system in 7 Days to Die
* Showing if the `ItemStack` is found or not in a search result.

### Using the ItemStack Controller

```XML
<Widget
    ...
    controller="Quartz.ItemStack, Quartz"
    lockedslot_color="color" - The color of that is returned to the {selectionbordercolor} binding if the slot is 
        locked

    search_color="color" - The color of that is returned to the {selectionbordercolor} binding if the slot contains
        an item that matches the search 

    nomatch_iconcolor="color" - The color that is returned to the {iconcolor} binding if the slot contains an item 
        that does not match the search. Also is used to tint the {durabilitycolor} of the item if in the slot has a
        durabilitycolor

    ...
/>
```

All those new attributes do not need to be set and only show up if they are set for that controller. The priority of
the color being returned in the {selectionbordercolor} binding is as followed

```text
 select_color > highlight_color > holding_color > search_color > lockedslot_color > background_color
```

So if the item is in a locked slot and the player selects the slot, the select_color color will be returned from the
{selectionbordercolor} instead of the lockedslot_color color.

### Additional ItemStack Bindings

XML Binding | Description
---------|----------
`{isempty}`                         | Is the slot empty
`{isalockedslot}`                   | Is the ItemStack a locked slot
`{issearchactive}`                  | Is there an active search going on by the player
`{matchessearch}`                   | Does the item in this slot matches the search by the player
`{itemql}`                          | The item's quality level. Returns "" if the item does not have durability
`{stackcount}`                      | How how many items are in the stack. Returns "" if the item has durability

## Quartz.Backpack

<p align="center" width="100%">
    <img width="33%" src="../images/Backpack.png"> <br>
    Quartz.Backpack being used in SMXui to show players which slots are <br>
    locked and what items match their search in their inventory
</p>

This controller is an extension of the vanilla `Backpack` controller. Both the Search and Slot locking features require
the child `ItemStack` controllers to be replaced with `Quartz.ItemStack`.

### Using the Backpack Controller

```XML
<Widget
    ...
    controller="Quartz.Backpack, Quartz"
    ...
>
```

#### Locked Slots

<p align="center" width="100%">
    <img width="33%" src="../images/BackpackLockable.gif"> <br>
    Quartz.Backpack being used in SMXui to show players which slots<br>
    and their items, match their search in their inventory
</p>

7 Days to Die has a inventory locking feature that is disabled in the vanilla UI. To enable the inventory locking,
a combobox with the `ComboBoxInt` controller has to be within the same window. The `Quartz.Backpack` controller will
use the first found widget with a `ComboBoxInt` controller attached to it.

#### Inventory Search

<p align="center" width="100%">
    <img width="33%" src="../images/BackpackSearch.gif"> <br>
    Quartz.Backpack being used in SMXui to show players which slots are <br>
    locked
</p>

To enable the inventory search, a widget with the `TextInput` controller has to be within the same window. It is possible to
use the vanilla `textfield` controls as the widget with the `TextInput` controller. The `Quartz.Backpack` controller will
use the first found widget with a `TextInput` controller attached to it.

## Quartz.LootContainer

This controller is an extension of the vanilla `LootContainer` controller. Both the Search and Slot locking features require the child ItemStack controllers to be `Quartz.ItemStack`.

### Using the LootContainer Controller

The usage is the same as the [`Quartz.Backpack`](#using-the-backpack-controller)

```XML
<Widget
    ...
    controller="Quartz.LootContainer, Quartz"
    ...
>
```

## Quartz.VehicleContainer

This controller is an extension of the vanilla `VehicleContainer` controller. Both the Search and Slot locking features require the child ItemStack controllers to be `Quartz.ItemStack`.

### Using the VehicleContainer Controller

The usage is the same as the [`Quartz.Backpack`](#using-the-backpack-controller)

```XML
<Widget
    ...
    controller="Quartz.VehicleContainer, Quartz"
    ...
>
```

## Quartz.MapInvitesListEntry

This controller is an extension of the vanilla `MapInvitesListEntry` controller.

### Using the MapInvitesListEntry Controller

```XML
<Widget
    ...
    controller="Quartz.MapInvitesListEntry, Quartz"
    ...
>
```

### Additional MapInvitesListEntry Bindings

XML Binding | Description
---------|----------
`{isempty}`                         | Is the entry empty

## Quartz.MapWaypointListEntry

This controller is an extension of the vanilla `MapWaypointListEntry` controller.

### Using the MapWaypointListEntry Controller

```XML
<Widget
    ...
    controller="Quartz.MapWaypointListEntry, Quartz"
    ...
>
```

### Additional MapWaypointListEntry Bindings

XML Binding | Description
---------|----------
`{isempty}`                         | Is the entry empty

## Quartz.RandomTexture

This controller allows replacing of a texture in a `<texture>` element randomly each time the window containing
the `<texture>` element is opened. For example, this can be used to replace the main menu background
every time the main menu is opened up.

### Using the RandomTexture Controller

```XML
<Widget
    ...
    controller="Quartz.RandomTexture, Quartz"

    textures="string" - The list of textures is used to pick which texture to return in the {randomtexture} binding. 
        Each new texture must be seperated by a `,`.
    ...
>
```

The controller can be placed in the `<texture>` element or any parent elements. And example of the string value in the
`textures` attribute is as followed

```Text
    @modfolder:Textures/test1.png,@modfolder:Textures/test2.png,Textures/UI/background
```

In the `<texture>` element, in the `texture` attribute, add the `{randomtexture}` binding, for example:

```XML
<texture
    ...
    texture="{randomtexture}"
    ...
>
```

### Additional RandomTexture Bindings

XML Binding | Description
---------|----------
`{randomtexture}`                         | The binding to fetch the random texture when the window is opened
