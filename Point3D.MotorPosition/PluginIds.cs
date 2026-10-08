using System;

namespace NINA.Point3d.MotorPosition {

    internal static class PluginIds {
        public const string MotorPosition = "8c8a1b94-345f-4399-8276-a6dd5c93d8aa";
        public static readonly Guid MotorPositionGuid = Guid.Parse(MotorPosition);

        /// <summary>The official Point3D plugin, whose saved camera view the dockable can follow.</summary>
        public static readonly Guid Point3DGuid = Guid.Parse("200ce2d2-6992-44fe-bf83-f8c2e01c7244");
    }
}
