using NetSdrControl;
using NetSdrControl.Protocol.HelpMessages;
using System.Net;

namespace NetSdrControlTests
{
    public class MockTcpClient : ITcpClient
    {
        public int ReceiveTimeout { get => _stream.ReceiveTimeout; set => _stream.ReceiveTimeout = value; }

        public int SendTimeout { get => _stream.SendTimeout; set => _stream.SendTimeout = value; }

        private bool _connected;
        public bool Connected => _connected;

        public string IpAddress { get; private set; }

        public MockTcpClient() 
        {
            _stream = new MockNetworkStream();
        }

        public void Close()
        {
            _connected = false;
        }

        public async ValueTask ConnectAsync(IPAddress ipAddress, int port, CancellationToken token)
        {
            IpAddress = ipAddress.ToString();
            await Task.Delay(2000);
            _connected = true;
        }

        public void Dispose()
        {
            _connected = false;
        }

        private MockNetworkStream _stream;
        public INetworkStream GetStream()
        {
            return _stream;
        }
    }

    public class MockNetworkStream : INetworkStream
    {
        public int SendTimeout { get; set; }
        public int ReceiveTimeout { get; set; }
        private Dictionary<string, string> _requestsToResponsesMap;
        private int _currentBytesLength;
        private string _currentBytesString;

        public MockNetworkStream()
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

        public async ValueTask<int> ReadAsync(byte[] bytes, CancellationToken token)
        {
            await Task.Delay(3000);
            if (!_requestsToResponsesMap.ContainsKey(_currentBytesString))
            {
                new NAKMessage().WriteToBuffer(bytes);
                return 2;
            }

            string response = _requestsToResponsesMap[_currentBytesString];
            var responseBytes = GetBytesFromString(response);
            for (int i = 0; i < responseBytes.Length; i++)
            {
                bytes[i] = responseBytes[i];
            }

            var currentBytesLength = _currentBytesLength;
            _currentBytesLength = 0;
            return currentBytesLength;
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

            _currentBytesString = GetStringFromBytes(bytes);
            _currentBytesLength = bytes.Length;
        }
    }
}
