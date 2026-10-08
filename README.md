# Point3D

## Issues / Requests

Please submit issues or requests here: [New Github Issue](https://github.com/FlyingKiwis/Nina.Point3D/issues/new/choose)

## About

This plugin adds a dockable window in the imaging view that shows a simulated model of your telescope.

## Point3D Motor Position

`Point3D.MotorPosition` is a separate plugin, with its own identifier, that installs alongside Point3D. It adds a Motor Position dockable that draws the mount from the absolute motor positions reported by the [OnStepX motor safety firmware](https://github.com/pardovot/OnStepX/tree/v10.24c-fram-motor-safety), independent of the pointing model and syncs.

- Polls `:PAGp#` over TCP. Host, port, poll interval and auto reconnect are in the plugin options.
- Connect and Disconnect are in the dockable. The model is dimmed while no motor data arrives.
- Site latitude for the model tilt comes from the connected telescope.
- "Follow Point3D view" copies the camera of Point3D's Telescope Model dockable, for side by side comparison.

It links the 3D viewport, models and resources from the Point3D project instead of copying them. A Debug build deploys it to `%LOCALAPPDATA%\NINA\Plugins\3.0.0\Point3D.MotorPosition`.

## Legal

- This plugin is distributed under the GPL v3 license.
    - [License](https://github.com/FlyingKiwis/Nina.Point3D/blob/master/LICENSE)
- It is primarily a port of Green Swamp Server's Point3D © 2021 Rob Morgan released under GPL v3
    - [Website](https://greenswamp.org/)
    - [License](https://github.com/rmorgan001/GS.Point3d/blob/master/LICENSE)
- Models and images redistributed with permission from Rob Morgan

## Contact

I'm in the [NINA discord](https://discord.gg/rWRbVbw) server as Kiwi🥝
