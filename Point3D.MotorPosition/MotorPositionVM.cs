using HelixToolkit.Wpf;
using NINA.Core.Utility;
using NINA.Equipment.Equipment.MyTelescope;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Point3D.Helpers;
using NINA.Profile;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.ViewModel;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand;
using MediaPoint3D = System.Windows.Media.Media3D.Point3D;
using Model3D = NINA.Point3D.Classes.Model3D;

namespace NINA.Point3d.MotorPosition {

    [Export(typeof(IDockableVM))]
    public class MotorPositionVM : DockableVM, ITelescopeConsumer {
        // Camera keys used by the Point3D plugin. This plugin stores its own camera under the same keys.
        private const string LookDirectionKey = "LookDirection";
        private const string UpDirectionKey = "UpDirection";
        private const string PositionKey = "Position";

        private static readonly Vector3D DefaultLookDirection = new Vector3D(43, 2157, -747);
        private static readonly Vector3D DefaultUpDirection = new Vector3D(0, 0.5, 0.85);
        private static readonly MediaPoint3D DefaultPosition = new MediaPoint3D(-135, -2324, 956);
        private static readonly TimeSpan DebugLogInterval = TimeSpan.FromMinutes(5);

        // PluginSettings raises PropertyChanged as "<plugin guid>-<key>" for every stored value.
        private static readonly string OwnSettingsPrefix = PluginIds.MotorPositionGuid + "-";
        private static readonly string Point3DSettingsPrefix = PluginIds.Point3DGuid + "-";

        private readonly ITelescopeMediator _telescopeMediator;
        private readonly IProfileService _profileService;
        private readonly MotorPositionSettings _settings;
        private readonly IPluginOptionsAccessor _viewOptions;
        private readonly IPluginOptionsAccessor _point3DViewOptions;

        private IProfile _activeProfile;
        private OnStepTcpClient _client;
        private double _siteLatitude;
        private bool? _isSouthernHemisphere;
        private Model3DType _otaStyle;
        private Color _modelColor;
        private DateTime _lastDebugLog = DateTime.MinValue;

        [ImportingConstructor]
        public MotorPositionVM(ITelescopeMediator telescopeMediator, IProfileService profileService) : base(profileService) {
            _telescopeMediator = telescopeMediator;
            _profileService = profileService;
            Title = "Motor Position";

            _settings = new MotorPositionSettings(profileService);
            _viewOptions = new PluginOptionsAccessor(profileService, PluginIds.MotorPositionGuid);
            _point3DViewOptions = new PluginOptionsAccessor(profileService, PluginIds.Point3DGuid);
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);

            _profileService.ProfileChanged += ProfileService_ProfileChanged;
            SubscribeToActiveProfile();

            LoadModel();
            LoadView(_viewOptions);
            _telescopeMediator.RegisterConsumer(this);
        }

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }

        private ConnectionState _connectionState = ConnectionState.Disconnected;
        public ConnectionState ConnectionState {
            get => _connectionState;
            private set {
                _connectionState = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ModelOn));
            }
        }

        public bool ModelOn => ConnectionState == ConnectionState.Connected;
        public Material Compass { get; private set; }
        public bool CameraVis { get; private set; } = false;
        public System.Windows.Media.Media3D.Model3D Model { get; private set; }

        private double _xAxis = 0;
        public double XAxis {
            get => _xAxis;
            set { if (_xAxis != value) { _xAxis = value; RaisePropertyChanged(); } }
        }

        private double _yAxis = 0;
        public double YAxis {
            get => _yAxis;
            set { if (_yAxis != value) { _yAxis = value; RaisePropertyChanged(); } }
        }

        private double _zAxis = 0;
        public double ZAxis {
            get => _zAxis;
            set { if (_zAxis != value) { _zAxis = value; RaisePropertyChanged(); } }
        }

        private bool _followPoint3DView = false;
        /// <summary>Copies the camera of the Point3D plugin's Telescope Model dockable whenever it moves.</summary>
        public bool FollowPoint3DView {
            get => _followPoint3DView;
            set {
                _followPoint3DView = value;
                RaisePropertyChanged();
                if (value) {
                    LoadView(_point3DViewOptions);
                }
            }
        }

        private Vector3D _lookDirection = DefaultLookDirection;
        public Vector3D LookDirection {
            get => _lookDirection;
            set {
                _lookDirection = value;
                _viewOptions.SetValueString(LookDirectionKey, value.ToString(CultureInfo.InvariantCulture));
                RaisePropertyChanged();
            }
        }

        private Vector3D _upDirection = DefaultUpDirection;
        public Vector3D UpDirection {
            get => _upDirection;
            set {
                _upDirection = value;
                _viewOptions.SetValueString(UpDirectionKey, value.ToString(CultureInfo.InvariantCulture));
                RaisePropertyChanged();
            }
        }

        private MediaPoint3D _position = DefaultPosition;
        public MediaPoint3D Position {
            get => _position;
            set {
                _position = value;
                _viewOptions.SetValueString(PositionKey, value.ToString(CultureInfo.InvariantCulture));
                RaisePropertyChanged();
            }
        }

        private void Connect() {
            Disconnect();
            var client = new OnStepTcpClient(_settings.Host, _settings.Port, TimeSpan.FromSeconds(_settings.PollIntervalSeconds), _settings.AutoReconnect);
            // A stopped client can still raise events while it winds down, so only the current client is listened to.
            client.StateChanged += state => {
                if (_client == client) {
                    ConnectionState = state;
                }
            };
            client.MotorPositionReceived += (axis1, axis2) => {
                if (_client == client) {
                    OnMotorPositionReceived(axis1, axis2);
                }
            };
            _client = client;
            client.Start();
        }

        private void Disconnect() {
            _client?.Stop();
            _client = null;
            ConnectionState = ConnectionState.Disconnected;
        }

        private void OnMotorPositionReceived(double axis1, double axis2) {
            var x = 90.0 - axis2 + _settings.XOffset;
            var y = axis1 - 90.0 + _settings.YOffset;
            var z = -Math.Abs(Volatile.Read(ref _siteLatitude)) + _settings.ZOffset;

            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                XAxis = x;
                YAxis = y;
                ZAxis = z;
            }));

            var message = $"MotorPosition: axis1={axis1:F2} axis2={axis2:F2} => X={x:F2} Y={y:F2} Z={z:F2}";
            if (DateTime.Now - _lastDebugLog >= DebugLogInterval) {
                _lastDebugLog = DateTime.Now;
                Logger.Debug(message);
            } else {
                Logger.Trace(message);
            }
        }

        public void UpdateDeviceInfo(TelescopeInfo deviceInfo) {
            Volatile.Write(ref _siteLatitude, deviceInfo.SiteLatitude);

            var southernHemisphere = deviceInfo.SiteLatitude < 0;
            if (_isSouthernHemisphere != southernHemisphere) {
                _isSouthernHemisphere = southernHemisphere;
                Compass = MaterialHelper.CreateImageMaterial(Model3D.GetCompassFile(southernHemisphere), 100);
                RaisePropertyChanged(nameof(Compass));
            }
        }

        public void Dispose() {
            Disconnect();
            _telescopeMediator.RemoveConsumer(this);
            _profileService.ProfileChanged -= ProfileService_ProfileChanged;
            UnsubscribeFromActiveProfile();
        }

        private void ProfileService_ProfileChanged(object sender, EventArgs e) {
            UnsubscribeFromActiveProfile();
            SubscribeToActiveProfile();
            LoadView(FollowPoint3DView ? _point3DViewOptions : _viewOptions);
            LoadModel();
        }

        private void SubscribeToActiveProfile() {
            _activeProfile = _profileService.ActiveProfile;
            if (_activeProfile != null) {
                _activeProfile.PluginSettings.PropertyChanged += PluginSettings_PropertyChanged;
            }
        }

        private void UnsubscribeFromActiveProfile() {
            if (_activeProfile != null) {
                _activeProfile.PluginSettings.PropertyChanged -= PluginSettings_PropertyChanged;
            }
        }

        private void PluginSettings_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var name = e.PropertyName ?? string.Empty;
            if (name.StartsWith(Point3DSettingsPrefix)) {
                if (FollowPoint3DView && IsViewKey(name.Substring(Point3DSettingsPrefix.Length))) {
                    LoadView(_point3DViewOptions);
                }
            } else if (name.StartsWith(OwnSettingsPrefix) && !IsViewKey(name.Substring(OwnSettingsPrefix.Length))) {
                LoadModel();
                if (_client != null) {
                    _client.AutoReconnect = _settings.AutoReconnect;
                }
            }
        }

        private static bool IsViewKey(string key) => key == LookDirectionKey || key == UpDirectionKey || key == PositionKey;

        /// <summary>Applies the camera saved in the given plugin's settings, keeping the current value for anything missing or unparseable.</summary>
        private void LoadView(IPluginOptionsAccessor source) {
            LookDirection = ParseOrKeep(source.GetValueString(LookDirectionKey, null), Vector3D.Parse, LookDirection);
            UpDirection = ParseOrKeep(source.GetValueString(UpDirectionKey, null), Vector3D.Parse, UpDirection);
            Position = ParseOrKeep(source.GetValueString(PositionKey, null), MediaPoint3D.Parse, Position);
        }

        private static T ParseOrKeep<T>(string text, Func<string, T> parse, T current) {
            if (string.IsNullOrEmpty(text)) {
                return current;
            }
            try {
                return parse(text);
            } catch (Exception ex) when (ex is FormatException || ex is InvalidOperationException) {
                Logger.Debug($"MotorPosition: cannot parse camera value '{text}', keeping {current}");
                return current;
            }
        }

        private void LoadModel() {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                try {
                    var otaStyle = _settings.OtaStyle;
                    var modelColor = _settings.ModelColor;
                    if (Model != null && otaStyle == _otaStyle && modelColor == _modelColor) {
                        return;
                    }

                    _otaStyle = otaStyle;
                    _modelColor = modelColor;
                    Model = Model3D.LoadTelescope(otaStyle, modelColor);
                    RaisePropertyChanged(nameof(Model));
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            }));
        }
    }
}
