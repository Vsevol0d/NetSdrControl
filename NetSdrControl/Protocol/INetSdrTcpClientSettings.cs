using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrControl.Protocol
{
    public interface INetSdrTcpClientSettings
    {
        int Port { get; }
        int MaxPackageSize { get; }
        int SendTimeoutMs { get; }
        int ReceiveTimeoutMs { get; }
    }
}
