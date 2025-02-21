using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrControlTests.UDP
{
    public class DefaultUdpClient : IUdpClient
    {
        private UdpClient _client;
        private string _ipAddress;

        public string? IPAddress => _ipAddress;

        public void Dispose()
        {
            _client?.Dispose();
        }

        public ValueTask<UdpReceiveResult> ReceiveAsync(CancellationToken cancellationToken)
        {
            if (_client == null)
            {
                return ValueTask.FromResult(new UdpReceiveResult());
            }
            return _client.ReceiveAsync(cancellationToken);
        }

        public void SetEndPoint(IPEndPoint endPoint)
        {
            _client = new UdpClient(endPoint);
            _ipAddress = endPoint.ToString();
        }
    }
}
