using NINA.Point3D.Helpers;
using NINA.Profile;
using NINA.Profile.Interfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace NINA.Point3d.MotorPosition {

    /// <summary>Profile backed plugin settings, used by both the options page and the dockable.</summary>
    public sealed class MotorPositionSettings : INotifyPropertyChanged {
        private const string DefaultHost = "192.168.0.1";
        private const int DefaultPort = 9998;
        private const int MinimumPollIntervalSeconds = 1;

        private readonly IPluginOptionsAccessor _options;

        public MotorPositionSettings(IProfileService profileService) {
            _options = new PluginOptionsAccessor(profileService, PluginIds.MotorPositionGuid);
            profileService.ProfileChanged += (sender, args) => RaisePropertyChanged(string.Empty);
        }

        public Model3DType OtaStyle {
            get => _options.GetValueEnum(nameof(OtaStyle), Model3DType.Default);
            set { _options.SetValueEnum(nameof(OtaStyle), value); RaisePropertyChanged(); }
        }

        public Color ModelColor {
            get => _options.GetValueColor(nameof(ModelColor), Colors.Blue);
            set { _options.SetValueColor(nameof(ModelColor), value); RaisePropertyChanged(); }
        }

        public double XOffset {
            get => _options.GetValueDouble(nameof(XOffset), 0);
            set { _options.SetValueDouble(nameof(XOffset), value); RaisePropertyChanged(); }
        }

        public double YOffset {
            get => _options.GetValueDouble(nameof(YOffset), 0);
            set { _options.SetValueDouble(nameof(YOffset), value); RaisePropertyChanged(); }
        }

        public double ZOffset {
            get => _options.GetValueDouble(nameof(ZOffset), 0);
            set { _options.SetValueDouble(nameof(ZOffset), value); RaisePropertyChanged(); }
        }

        public string Host {
            get => _options.GetValueString(nameof(Host), DefaultHost);
            set { _options.SetValueString(nameof(Host), value); RaisePropertyChanged(); }
        }

        public int Port {
            get => _options.GetValueInt32(nameof(Port), DefaultPort);
            set { _options.SetValueInt32(nameof(Port), value); RaisePropertyChanged(); }
        }

        public int PollIntervalSeconds {
            get => Math.Max(MinimumPollIntervalSeconds, _options.GetValueInt32(nameof(PollIntervalSeconds), MinimumPollIntervalSeconds));
            set { _options.SetValueInt32(nameof(PollIntervalSeconds), value); RaisePropertyChanged(); }
        }

        public bool AutoReconnect {
            get => _options.GetValueBoolean(nameof(AutoReconnect), false);
            set { _options.SetValueBoolean(nameof(AutoReconnect), value); RaisePropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
