# Upgrading from XUiComponents

To upgrade to Quartz from XUiComponents follow the steps below. If you are not packaging the
XUiComponents dlls with your mod skip to Step Three.

## Step One

Replace both XUiComponents.dll and XUiComponentsLoader.dll in your mod's folder with
Quartz.dll and QuartzLoader.dll if you are packaging those dlls with your mod.

If only XUiComponents.dll is packaged in your mod's folder, follow Step Two, otherwise skip
to Step Three

## Step Two

In the class that extends IModApi replace the following line

```C#
    XUiComponents.LoadXuiComponents(harmony);
```

with

```C#
    QuartzMod.LoadQuartz(harmony);
```

## Step Three

With the the branding, the namespace for each controller has changed, along with the name of
the assembly. So each reference of one of the controllers provided in the XUiComponents.dll
in your mod's xml will have to be replaced with the new references. It is as simple as replacing each
instance of

```XML
<Widget
    ...
    controller="XUiC.{Name of the controller}, XUiComponents"
    ...
/>
```

with

```XML
<Widget
    ...
    controller="Quartz.{Name of the controller}, Quartz"
    ...
/>
```
