using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrControl.DataItems
{
    public interface INetSdrUdpClient
    {
        Task<byte[]> ReadPacket(CancellationToken cancellationToken);
    }
}
