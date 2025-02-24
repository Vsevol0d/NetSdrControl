using System.Net;

namespace NetSdrControl.Interfaces
{
    public interface ITcpClient
    {
        int ConnectTimeMs { get; set; }
        int ReceiveTimeout { get; set; }
        int SendTimeout { get; set; }
        bool Connected { get; }
        string IpAddress { get; }

        ValueTask ConnectAsync(IPAddress ipAddress, int port, CancellationToken token);
        INetworkStream GetStream();
        void Close();
        void Dispose();
        ITcpClient Clone();
    }

    public interface INetworkStream
    {
        bool DataAvailable { get; }

        Task<int> ReadAsync(byte[] bytes, int offset, int bytesCountToRead, CancellationToken token);
        ValueTask WriteAsync(byte[] bytes, CancellationToken token);
    }
}
