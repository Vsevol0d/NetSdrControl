using NetSdrControl.Interfaces;
using NetSdrControl.Protocol.HelpMessages;
using System.Net;

namespace NetSdrControlTests
{
    public class FakeTcpClient : ITcpClient
    {
        public int ReceiveTimeout { get => _stream.ReceiveTimeout; set => _stream.ReceiveTimeout = value; }

        public int SendTimeout { get => _stream.SendTimeout; set => _stream.SendTimeout = value; }

        private bool _connected;
        public bool Connected => _connected;

        public string IpAddress { get; private set; }

        public int ConnectTimeMs { get; set; } = 5000;

        public FakeTcpClient() 
        {
            _stream = new FakeNetworkStream();
        }

        public void Close()
        {
            _connected = false;
        }

        public async virtual ValueTask ConnectAsync(IPAddress ipAddress, int port, CancellationToken token)
        {
            IpAddress = ipAddress.ToString();
            await Task.Delay(ConnectTimeMs, token);
            _connected = true;
        }

        public void Dispose()
        {
            _connected = false;
        }

        private FakeNetworkStream _stream;
        public INetworkStream GetStream()
        {
            return _stream;
        }

        public ITcpClient Clone()
        {
            return new FakeTcpClient() { ConnectTimeMs = ConnectTimeMs };
        }
    }

    public class FakeNetworkStream : INetworkStream
    {
        public int SendTimeout { get; set; }
        public int ReceiveTimeout { get; set; }
        private Dictionary<string, string> _requestsToResponsesMap;
        private int _currentBytesLength;
        private string _currentBytesString;
        private byte[] _currentBytes;

        public FakeNetworkStream()
        {
            _currentBytesLength = 0;
            _requestsToResponsesMap = new Dictionary<string, string>()
            {
                { "0820180080028000", "0820180080028000" },
                { "0800180080028000", "0800180080020000" },
                { "0800180000010000", "0800180000010000" },
                { "0A0020000090C6D50000", "0A0020000090C6D50000" },
                { "0520200002", "0A0020000290C6D50000" }
            };
        }

        public bool DataAvailable => _currentBytesLength > 0;

        public async Task<int> ReadAsync(byte[] bytes, int offset, int bytesCountToRead, CancellationToken token)
        {
            await Task.Delay(1000);
            if (!_requestsToResponsesMap.ContainsKey(_currentBytesString))
            {
                new NAKMessage().WriteToBuffer(bytes);
                return 2;
            }

            string response = _requestsToResponsesMap[_currentBytesString];
            var responseBytes = GetBytesFromString(response);
            for (int i = offset; i < offset + bytesCountToRead; i++)
            {
                bytes[i] = responseBytes[i];
            }

            _currentBytesLength -= bytesCountToRead;
            return bytesCountToRead;
        }

        private byte[] GetBytesFromString(string str)
        {
            return Convert.FromHexString(str);
        }

        private string GetStringFromBytes(byte[] bytes)
        {
            return Convert.ToHexString(bytes);
        }

        public async ValueTask WriteAsync(byte[] bytes, CancellationToken token)
        {
            await Task.Delay(3000);

            _currentBytes = bytes;
            _currentBytesString = GetStringFromBytes(bytes);
            _currentBytesLength = bytes.Length;
        }
    }
}
