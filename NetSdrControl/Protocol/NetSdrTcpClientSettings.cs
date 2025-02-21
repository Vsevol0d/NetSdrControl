using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrControl.Protocol
{
    public class NetSdrTcpClientSettings : INetSdrTcpClientSettings
    {
        public int Port { get; } = 50000;
        public int MaxPackageSize { get; } = 8192;
        public int SendTimeoutMs { get; } = 30000;
        public int ReceiveTimeoutMs { get; } = 30000;
    }
}
