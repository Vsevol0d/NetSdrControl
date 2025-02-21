using NetSdrControlTests.UDP;
using System.Net;
using System.Net.Sockets;

namespace NetSdrControlTests
{
    public class MockUdpClient : IUdpClient
    {
        public string? IPAddress => "127.0.0.1";

        public void Dispose()
        {
            // Just do nothing
        }

        public ValueTask<UdpReceiveResult> ReceiveAsync(CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(new UdpReceiveResult([1, 2, 3], null));
        }

        public void SetEndPoint(IPEndPoint endPoint)
        {
            // Just do nothing
        }
    }
}
