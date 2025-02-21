using NetSdrControl.ControlItems;
using NetSdrControl.Protocol.HelpMessages;
using System.Collections.Concurrent;
using System.IO;
using System.Net;

namespace NetSdrControl.Protocol
{
    public class NetSdrTcpClient<TTcpClient> : INetSdrTcpClient where TTcpClient : class, ITcpClient, new()
    {
        public event EventHandler<byte[]> OnControlItemChanged = delegate { };

        public bool IsConnected => _tcpClient != null && _tcpClient.Connected;

        public string IpAddress => _tcpClient.IpAddress;

        private readonly int _port;
        private readonly int _receiveTimeoutMs;
        private readonly int _sendTimeoutMs;
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
        private TTcpClient _tcpClient;

        public NetSdrTcpClient(INetSdrTcpClientSettings settings, TTcpClient tcpClient, IBitDecoder bitDecoder, IControlItemHeader controlItemHeader)
        {
            _tcpClient = tcpClient;
            _bitDecoder = bitDecoder;
            _controlItemHeader = controlItemHeader;

            _port = settings.Port;
            _maxPackageSize = settings.MaxPackageSize;
            _receiveTimeoutMs = settings.ReceiveTimeoutMs;
            _sendTimeoutMs = settings.SendTimeoutMs;

            _responseBuffer = new byte[_maxPackageSize];
            _restReadBuffer = new byte[_maxPackageSize];
            _restReadBufferLength = 0;
            _receiveMessages = new ConcurrentQueue<byte[]>();
            _transmitMessages = new ConcurrentQueue<ControlItemCommandData>();
            _disconnectionSource = new CancellationTokenSource();
        }

        public Task<byte[]> Send(byte[] messageBytes)
        {
            if (!IsConnected)
            {
                throw new Exception("Connection not active. Connect first");
            }
            var command = new ControlItemCommandData(messageBytes, new TaskCompletionSource<byte[]>());
            _transmitMessages.Enqueue(command);
            return command.CompletionSource.Task;
        }

        public async Task<bool> Connect(string ipAddress)
        {
            try
            {
                Console.WriteLine("Connecting...");

                _tcpClient = new TTcpClient();
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
                        OnControlItemChanged?.Invoke(this, messageBytes);
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
                    else
                    {
                        var stream = _tcpClient.GetStream();
                        await ReadAsync(stream, _responseBuffer, _disconnectionSource.Token);

                        // Check for unsolicited items when there are no active commands to execute
                        while (CheckForUnsolicitedUpdate(_responseBuffer))
                        {
                            var messageBytes = GetMessageBytes(_responseBuffer);
                            _receiveMessages.Enqueue(messageBytes);

                            await ReadAsync(stream, _responseBuffer, _disconnectionSource.Token);
                        }
                    }
                }
            });
        }

        private async ValueTask ReadAsync(INetworkStream stream, byte[] responseBuffer, CancellationToken token)
        {
            if (stream.DataAvailable)
            {
                await stream.ReadAsync(responseBuffer, token);
            }
            // Do reading only if there is some data present in the stream in order to not block stream writings as read/write cycle runs synchronously
            /*if (_restReadBufferLength >= 2)
            {

            }
            if (stream.DataAvailable)
            {
                bool allMessageBytesCame = false;
                bool isHeaderRead = false;
                int totalBytesReceived = _restReadBufferLength;
                int messageLength = 0;

                while (!allMessageBytesCame)
                {
                    //_restReadBufferPointerIndex
                    while (_controlItemHeader.HeaderSize > totalBytesReceived)
                    {
                        int bytesRead = await stream.ReadAsync(responseBuffer, token);
                        responseBuffer.CopyTo(_restReadBuffer, bytesRead);
                        totalBytesReceived += bytesRead;
                        Array.Copy(responseBuffer, 0, _restReadBuffer, totalBytesReceived, bytesRead);
                        _restReadBufferLength = totalBytesReceived + bytesRead;
                    }

                    if (!isHeaderRead)
                    {
                        messageLength = _controlItemHeader.DecodeMessageLength(_restReadBuffer[0], _restReadBuffer[1]);
                        isHeaderRead = true;
                    }

                    if (messageLength < _restReadBufferLength)
                    {
                        _restReadBufferLength -= messageLength;
                    }
                }
            }*/
        }

        private void StopThreads()
        {
            _disconnectionSource.Cancel();
        }

        private void DisposeClient()
        {
            _tcpClient.Close();
            _tcpClient.Dispose();
            _tcpClient = null;
        }
        public void Disconnect()
        {
            StopThreads();
            DisposeClient();
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
                await stream.WriteAsync(message.MessageBytes, token);        // Send control item message to tcp socket
                await ReadAsync(stream, _responseBuffer, token);             // Ensure that requests are synchronized with responses as we have no garantee that Target will process our Host requests consequently and will not ommit some requests

                // Check if there any unsolicited items were added during command sending
                while (CheckForUnsolicitedUpdate(_responseBuffer))
                {
                    var messageBytes = GetMessageBytes(_responseBuffer);
                    _receiveMessages.Enqueue(messageBytes);

                    await ReadAsync(stream, _responseBuffer, token);
                }

                Console.WriteLine($"Command succeded");
                var resultBytes = GetMessageBytes(_responseBuffer);
                message.CompletionSource.SetResult(resultBytes);
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
            return messageType == (byte)RequestResponseKind.Unsolicited;
        }
    }
}
