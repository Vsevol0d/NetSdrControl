using NetSdrControl.ControlItems;
using NetSdrControl.Interfaces;
using NetSdrControl.Protocol.Interfaces;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace NetSdrControl.Protocol
{
    public class NetSdrHost : INetSdrHost
    {
        public event EventHandler<byte[]> ControlItemChanged = delegate { };

        public bool IsConnected => _tcpClient != null && _tcpClient.Connected;

        public string IpAddress => _tcpClient.IpAddress;

        private readonly int _port;
        private readonly int _receiveTimeoutMs;
        private readonly int _sendTimeoutMs;
        private readonly int _connectTimeout;
        private readonly int _maxPackageSize;
        private CancellationTokenSource _disconnectionSource;
        private ConcurrentQueue<ControlItemCommandData> _transmitMessages;
        private ConcurrentQueue<byte[]> _receiveMessages;
        private byte[] _responseBuffer;
        private byte[] _restReadBuffer;
        private int _restReadBufferPointerIndex;
        private int _restReadBufferLength;
        private IBitDecoder _bitDecoder;
        private IControlItemHeader _controlItemHeader;
        private ITcpClient _tcpClient;
        private readonly SemaphoreSlim _connectDisconnectSyncControl = new SemaphoreSlim(1, 1);
        private System.Timers.Timer _connectTimeoutTimer;
        private Dictionary<ushort, (int StartByteOffset, byte[] BitsIndices)[]> _commandsIdProvider;

        /// <summary>
        /// Target could process messages sent by Host in random order, so we can get into a situation when two different similar commands
        /// like GetFrequency(Channel_1) and GetFrequency(Channel_2) will return messed up values to the Host listeners, so we need to differentiate
        /// all the commands. Only the same commands don't matters the order because returns the same(or at least the first actual fetched) values
        /// </summary>
        private Dictionary<string, ControlItemCommandData> _executingCommandsTaskSources;

        public NetSdrHost(INetSdrTcpClientSettings settings, ITcpClient tcpClient, IBitDecoder bitDecoder, IControlItemHeader controlItemHeader)
        {
            _tcpClient = tcpClient;
            _bitDecoder = bitDecoder;
            _controlItemHeader = controlItemHeader;

            _port = settings.Port;
            _maxPackageSize = settings.MaxPackageSize;
            _receiveTimeoutMs = settings.ReceiveTimeoutMs;
            _sendTimeoutMs = settings.SendTimeoutMs;
            _connectTimeout = settings.ConnectTimeout;

            _responseBuffer = new byte[_maxPackageSize];
            _restReadBuffer = new byte[_maxPackageSize];
            _restReadBufferLength = 0;
            _connectTimeoutTimer = new System.Timers.Timer(_connectTimeout);
            _connectTimeoutTimer.Elapsed += HandleTimerElapsed;
            
            _receiveMessages = new ConcurrentQueue<byte[]>();
            _transmitMessages = new ConcurrentQueue<ControlItemCommandData>();
            _disconnectionSource = new CancellationTokenSource();
            _executingCommandsTaskSources = new Dictionary<string, ControlItemCommandData>();
            _commandsIdProvider = new Dictionary<ushort, (int StartByteOffset, byte[] BitsIndices)[]>();
        }

        private void HandleTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            _disconnectionSource.Cancel();
        }

        public Task<byte[]> Send(byte[] messageBytes, (int StartByteOffset, byte[] BitsIndices)[] identifierBits)
        {
            if (!IsConnected)
            {
                throw new NetSdrHostException(NetSdrHostException.SendException);
            }

            var commandCode = _controlItemHeader.DecodeMessageCode(messageBytes[2], messageBytes[3]);

            if (!_commandsIdProvider.ContainsKey(commandCode))
            {
                _commandsIdProvider[commandCode] = identifierBits;
            }
            string messageId = MessageId.DecodeFromMessageBytes(messageBytes, identifierBits);
            _executingCommandsTaskSources[messageId] = new ControlItemCommandData(messageBytes, new TaskCompletionSource<byte[]>());

            var command = _executingCommandsTaskSources[messageId];
            _transmitMessages.Enqueue(command);
            return command.CompletionSource.Task;
        }

        public async virtual Task<bool> Connect(string ipAddress)
        {
            if (IsConnected)
            {
                return true;
            }

            await _connectDisconnectSyncControl.WaitAsync();

            if (IsConnected)
            {
                _connectDisconnectSyncControl.Release();
                return true;
            }

            _connectTimeoutTimer = new System.Timers.Timer(_connectTimeout);
            _connectTimeoutTimer.Elapsed += HandleTimerElapsed;
            _connectTimeoutTimer.Start();
            try
            {
                Console.WriteLine("Connecting...");

                _tcpClient.ReceiveTimeout = _receiveTimeoutMs;
                _tcpClient.SendTimeout = _sendTimeoutMs;
                _disconnectionSource = new CancellationTokenSource();

                await _tcpClient.ConnectAsync(IPAddress.Parse(ipAddress), _port, _disconnectionSource.Token);
                StartThreads();
                Console.WriteLine($"Connected to {ipAddress}:{_port} successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection was failed, try to reconnect later. Reason:" + ex.Message);
                DisposeClient();
                return false;
            }
            finally 
            {
                _connectTimeoutTimer.Stop();
                _connectDisconnectSyncControl.Release(); 
            }
        }

        private void StartThreads()
        {
            // Process unsolicited updates in separate thread to avoid possible hangings or crashes in main read/write cycle
            Task.Run(() =>
            {
                while (!_disconnectionSource.IsCancellationRequested)
                {
                    if (_receiveMessages.TryDequeue(out var messageBytes))
                    {
                        ControlItemChanged?.Invoke(this, messageBytes);
                    }
                }
            });

            // Start commands read/write
            Task.Run(async () =>
            {
                while (!_disconnectionSource.IsCancellationRequested)
                {
                    if (_transmitMessages.TryDequeue(out var message))
                    {
                        await SendCommand(message, _disconnectionSource.Token);
                    }
                    Thread.Sleep(500);
                }
            });

            Task.Run(async () =>
            {
                while (!_disconnectionSource.IsCancellationRequested)
                {
                    var stream = _tcpClient.GetStream();
                    int bytesRead = await ReadAsync(stream, _responseBuffer, _disconnectionSource.Token);

                    // Check for unsolicited items when there are no active commands to execute
                    while (CheckForUnsolicitedUpdate(_responseBuffer))
                    {
                        var messageBytes = GetMessageBytes(_responseBuffer);
                        _receiveMessages.Enqueue(messageBytes);

                        bytesRead = await ReadAsync(stream, _responseBuffer, _disconnectionSource.Token);
                    }

                    if (bytesRead > _controlItemHeader.HeaderAndCodeSize)
                    {
                        var resultBytes = GetMessageBytes(_responseBuffer);
                        ushort messageCode = _controlItemHeader.DecodeMessageCode(resultBytes[2], resultBytes[3]);
                        var mask = _commandsIdProvider[messageCode];
                        string messageId = MessageId.DecodeFromMessageBytes(resultBytes, mask);
                        var commandData = _executingCommandsTaskSources[messageId];
                        commandData.CompletionSource.SetResult(resultBytes);
                    }

                    Thread.Sleep(100);
                }
            });
        }

        private async Task<int> ReadAsync(INetworkStream stream, byte[] responseBuffer, CancellationToken token)
        {
            if (stream.DataAvailable)
            {
                // Read the header first
                await stream.ReadAsync(responseBuffer, 0, _controlItemHeader.HeaderSize, token);
                ushort messageLength = _controlItemHeader.DecodeMessageLength(responseBuffer[0], responseBuffer[1]);
                int payloadLength = messageLength - _controlItemHeader.HeaderSize;

                // Read other message part
                return await stream.ReadAsync(responseBuffer, _controlItemHeader.HeaderSize, payloadLength, token);
            }

            return 0;
        }

        private void StopThreads()
        {
            _disconnectionSource.Cancel();
        }

        private void DisposeClient()
        {
            var newClone = _tcpClient.Clone();

            _tcpClient.Close();
            _tcpClient.Dispose();
            _connectTimeoutTimer.Stop();
            _connectTimeoutTimer.Dispose();

            _tcpClient = newClone;
        }
        public async Task Disconnect()
        {
            await _connectDisconnectSyncControl.WaitAsync();
            try
            {
                StopThreads();
                DisposeClient();
            }
            finally
            {
                _connectDisconnectSyncControl.Release();
            }
        }

        private byte[] GetMessageBytes(byte[] responseBuffer)
        {
            ushort messageSize = _controlItemHeader.DecodeMessageLength(responseBuffer[0], responseBuffer[1]);
            var messageBytes = new byte[messageSize];
            Array.Copy(responseBuffer, messageBytes, messageSize);
            return messageBytes;
        }

        private async Task SendCommand(ControlItemCommandData message, CancellationToken token)
        {
            try
            {
                if (!IsConnected)
                {
                    Console.WriteLine("Tcp client is not connected. Try to 'Connect' first");
                    return;
                }

                Console.WriteLine($"Sending command");
                var stream = _tcpClient.GetStream();
                await stream.WriteAsync(message.MessageBytes, token);

                Console.WriteLine($"Command succeded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Command failed");
                message.CompletionSource.SetException(ex);
            }
        }

        private bool CheckForUnsolicitedUpdate(byte[] responseBuffer)
        {
            byte messageType = _bitDecoder.DecodeByteByMask(responseBuffer, 1, [7, 6, 5]);
            return messageType == (byte)RequestResponseType.Unsolicited;
        }
    }

    public class NetSdrHostException : Exception
    {
        public static string SendException = "Connection not active. Connect first";

        public NetSdrHostException(string exception) : base(exception)
        {
        }
    }
}
