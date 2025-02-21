using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrControlTests.UDP
{
    public interface IUdpClient : IDisposable
    {
        string? IPAddress { get; }

        ValueTask<UdpReceiveResult> ReceiveAsync(CancellationToken cancellationToken);
        void SetEndPoint(IPEndPoint endPoint);
    }
}
