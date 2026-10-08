using NINA.Point3d.MotorPosition;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: Guid(PluginIds.MotorPosition)]

[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]

[assembly: AssemblyTitle("Point3D Motor Position")]
[assembly: AssemblyDescription("Displays a model of the mount from OnStepX absolute motor positions")]

[assembly: AssemblyCompany("pardovot")]
[assembly: AssemblyProduct("Point3D Motor Position")]
[assembly: AssemblyCopyright("Copyright © 2023 Drew McDermott, 2026 pardovot")]

[assembly: AssemblyMetadata("MinimumApplicationVersion", "3.0.0.1085")]

[assembly: AssemblyMetadata("License", "GPL-3.0")]
[assembly: AssemblyMetadata("LicenseURL", "https://www.gnu.org/licenses/gpl-3.0.en.html")]
[assembly: AssemblyMetadata("Repository", "https://github.com/pardovot/Nina.Point3D")]
[assembly: AssemblyMetadata("Homepage", "https://github.com/pardovot/Nina.Point3D")]
[assembly: AssemblyMetadata("Tags", "Point3D,OnStepX,Telescope,Model,Motor,Safety")]
[assembly: AssemblyMetadata("ChangelogURL", "")]
[assembly: AssemblyMetadata("FeaturedImageURL", "")]
[assembly: AssemblyMetadata("ScreenshotURL", "")]
[assembly: AssemblyMetadata("AltScreenshotURL", "")]
[assembly: AssemblyMetadata("LongDescription", @"# Point3D Motor Position

Adds a Motor Position dockable that draws the mount from the absolute motor positions reported by the
[OnStepX motor safety firmware](https://github.com/pardovot/OnStepX/tree/v10.24c-fram-motor-safety) (`:PAGp#` over TCP),
independent of the pointing model and syncs. Installs alongside the Point3D plugin and can follow its Telescope Model camera.

## Legal

- Distributed under the GPL v3 license.
- Based on [Point3D for N.I.N.A.](https://github.com/FlyingKiwis/Nina.Point3D) by Drew McDermott, a port of Green Swamp Server's Point3D © 2021 Rob Morgan, both GPL v3.
- Models and images redistributed with permission from Rob Morgan.")]

[assembly: ComVisible(false)]
