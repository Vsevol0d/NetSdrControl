using System.Net.Sockets;
using System.Net;

namespace NetSdrControl.Interfaces
{
    public interface IUdpClient : IDisposable
    {
        string? IPAddress { get; }

        ValueTask<UdpReceiveResult> ReceiveAsync(CancellationToken cancellationToken);
        void SetEndPoint(IPEndPoint endPoint);
    }
}
