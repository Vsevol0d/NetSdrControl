using System.Net;

namespace NetSdrControl
{
    public interface ITcpClient
    {
        int ReceiveTimeout { get; set; }
        int SendTimeout { get; set; }
        bool Connected { get; }
        string IpAddress { get; }

        ValueTask ConnectAsync(IPAddress ipAddress, int port, CancellationToken token);
        INetworkStream GetStream();
        void Close();
        void Dispose();
    }

    public interface INetworkStream
    {
        bool DataAvailable { get; }

        ValueTask<int> ReadAsync(byte[] bytes, CancellationToken token);
        ValueTask WriteAsync(byte[] bytes, CancellationToken token);
    }
}
