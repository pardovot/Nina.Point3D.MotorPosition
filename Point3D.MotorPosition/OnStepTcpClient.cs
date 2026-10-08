using NINA.Core.Utility;
using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Point3d.MotorPosition {

    public enum ConnectionState {
        Disconnected,
        Connecting,
        Connected
    }

    /// <summary>
    /// Polls OnStepX for its absolute motor positions. Each poll opens a new connection, so this also works
    /// with command servers that close the socket after every reply.
    /// </summary>
    internal sealed class OnStepTcpClient {
        private const string MotorPositionCommand = ":PAGp#";
        private const char ReplyTerminator = '#';
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(2);

        private readonly string _host;
        private readonly int _port;
        private readonly TimeSpan _pollInterval;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private volatile bool _autoReconnect;
        private ConnectionState _state = ConnectionState.Disconnected;

        public OnStepTcpClient(string host, int port, TimeSpan pollInterval, bool autoReconnect) {
            _host = host;
            _port = port;
            _pollInterval = pollInterval;
            _autoReconnect = autoReconnect;
        }

        /// <summary>Raised with axis1 and axis2 in degrees.</summary>
        public event Action<double, double> MotorPositionReceived;

        public event Action<ConnectionState> StateChanged;

        public bool AutoReconnect {
            get => _autoReconnect;
            set => _autoReconnect = value;
        }

        public void Start() {
            SetState(ConnectionState.Connecting);
            Task.Run(() => PollLoop(_cancellation.Token));
        }

        public void Stop() => _cancellation.Cancel();

        private async Task PollLoop(CancellationToken cancellationToken) {
            Logger.Info($"MotorPosition: polling {_host}:{_port} every {_pollInterval.TotalSeconds}s");
            while (!cancellationToken.IsCancellationRequested) {
                try {
                    var reply = await RequestMotorPosition(cancellationToken);
                    Logger.Trace($"MotorPosition: reply '{reply}'");
                    SetState(ConnectionState.Connected);

                    if (TryParseMotorPosition(reply, out var axis1, out var axis2)) {
                        MotorPositionReceived?.Invoke(axis1, axis2);
                    } else {
                        Logger.Warning($"MotorPosition: unexpected reply '{reply}'");
                    }

                    await Task.Delay(_pollInterval, cancellationToken);
                } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                    break;
                } catch (Exception ex) {
                    Logger.Warning($"MotorPosition: {_host}:{_port} {ex.Message}");
                    if (!AutoReconnect) {
                        break;
                    }
                    SetState(ConnectionState.Connecting);
                    try {
                        await Task.Delay(ReconnectDelay, cancellationToken);
                    } catch (OperationCanceledException) {
                        break;
                    }
                }
            }
            SetState(ConnectionState.Disconnected);
        }

        private async Task<string> RequestMotorPosition(CancellationToken cancellationToken) {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(RequestTimeout);
            try {
                using var tcp = new TcpClient();
                await tcp.ConnectAsync(_host, _port, timeout.Token);
                var stream = tcp.GetStream();
                await stream.WriteAsync(Encoding.ASCII.GetBytes(MotorPositionCommand), timeout.Token);
                return await ReadReply(stream, timeout.Token);
            } catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
                throw new TimeoutException($"no reply within {RequestTimeout.TotalSeconds}s");
            }
        }

        private static async Task<string> ReadReply(NetworkStream stream, CancellationToken cancellationToken) {
            var reply = new StringBuilder();
            var buffer = new byte[1];
            while (true) {
                if (await stream.ReadAsync(buffer, cancellationToken) == 0) {
                    throw new IOException("connection closed before the end of the reply");
                }
                var character = (char)buffer[0];
                if (character == ReplyTerminator) {
                    return reply.ToString();
                }
                reply.Append(character);
            }
        }

        /// <summary>Parses "+aaa.aa,+bbb.bb", axis1 and axis2 in degrees.</summary>
        private static bool TryParseMotorPosition(string reply, out double axis1, out double axis2) {
            axis1 = axis2 = 0;
            var parts = reply.Split(',');
            return parts.Length == 2
                && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out axis1)
                && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out axis2);
        }

        private void SetState(ConnectionState state) {
            if (_state == state) {
                return;
            }
            _state = state;
            StateChanged?.Invoke(state);
        }
    }
}
