# Using the new widgets in your mod

Once XUiComponents is added to your mod as shown in the [README.md](../README.md),
you can start using the new widgets in your mod.

## The widgets

* [`curvedlabel`](#Curved-Label) A label that curves the text
* [`scrollview`](#Scroll-view) A widget container that is scrollable
* [`scrollbar`](#Scrollbar) A scrollbar that is usable with the `scrollview` to scroll the `scrollview`

## Curved Label

<p align="center" width="100%">
    <img width="33%" src="../images/CurvedLabel1.png"> <br>
    A curved label displayed in game
</p>

The Curved Label is an extension of the Label widget that allows text to be curved around a central point.

### Using the Curved Label

The widget is added the same way as other widgets, where it is defined in the xml as its own element.

```XML
<curvedlabel
...
radius="float" - The radius from the center point to place the first line of text
arc_degrees="float" - How much the text should curve around the center point
angular_offset="float" - The offset to rotate the text
max_degrees_per_letter="integer" - The max spacing between the letters based on degrees
flip="true|false" - If the first line should be at top line or bottom line, 
    default is false
draw_outwards="true|false" - If the text should be placed going towards the center point or away from the center point,
    default is false
justify="left|right|center" - Where the text should justify towards. If left, the text will expand clockwise, right, the
    text will expand counter clockwise, center the text will expand in both directions
...
/>
```

<p align="center" width="100%">
    <img width="33%" src="../images/CurvedLabel2.png"> <br>
    A curved label displayed in game with <br>
    flip="true" and draw_outwards="true"
</p>

## Scroll View

## Scrollbar
