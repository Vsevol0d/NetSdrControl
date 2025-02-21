using System.Net.Sockets;
using System.Net;

namespace NetSdrControl
{
    public class DefaultNetworkStream : INetworkStream
    {
        private NetworkStream _networkStream;
        public DefaultNetworkStream(NetworkStream networkStream)
        {
            _networkStream = networkStream;
        }

        public bool DataAvailable => _networkStream.DataAvailable;

        public ValueTask<int> ReadAsync(byte[] bytes, CancellationToken token)
        {
            return _networkStream.ReadAsync(bytes, token);
        }

        public ValueTask WriteAsync(byte[] bytes, CancellationToken token)
        {
            return _networkStream.WriteAsync(bytes, token);
        }
    }

    public class DefaultTcpClient : ITcpClient
    {
        public int ReceiveTimeout { get => _tcpClient.ReceiveTimeout; set => _tcpClient.ReceiveTimeout = value; }
        public int SendTimeout { get => _tcpClient.SendTimeout; set => _tcpClient.SendTimeout = value; }

        public bool Connected => _tcpClient.Connected;

        public string IpAddress { get; private set; }

        private TcpClient _tcpClient;
        public DefaultTcpClient()
        {
            _tcpClient = new TcpClient();
        }

        public void Close()
        {
            _tcpClient.Close();
        }

        public ValueTask ConnectAsync(IPAddress ipAddress, int port, CancellationToken token)
        {
            IpAddress = ipAddress.ToString();
            return _tcpClient.ConnectAsync(ipAddress, port, token);
        }

        public void Dispose()
        {
            _tcpClient.Dispose();
        }

        public INetworkStream GetStream()
        {
            return new DefaultNetworkStream(_tcpClient.GetStream());
        }
    }
}
