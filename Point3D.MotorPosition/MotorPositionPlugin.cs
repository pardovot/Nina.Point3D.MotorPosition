using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Point3D.Helpers;
using NINA.Profile.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Model3D = NINA.Point3D.Classes.Model3D;

namespace NINA.Point3d.MotorPosition {

    [Export(typeof(IPluginManifest))]
    public class MotorPositionPlugin : PluginBase {

        [ImportingConstructor]
        public MotorPositionPlugin(IProfileService profileService) {
            Settings = new MotorPositionSettings(profileService);
        }

        public MotorPositionSettings Settings { get; }

        public Dictionary<string, Color> ModelColors => Model3D.ModelColors;

        public Dictionary<string, Model3DType> OTAStyles => Model3D.OTAStyles;
    }
}
