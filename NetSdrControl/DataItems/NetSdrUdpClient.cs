using System.Net;
using NetSdrControl.Interfaces;

namespace NetSdrControl.DataItems
{
    public class NetSdrUdpClient<TUdpClient> : INetSdrUdpClient where TUdpClient : class, IUdpClient, new()
    {
        private int _port;
        private string? _ipAddress;
        private IUdpClient _client;

        public NetSdrUdpClient(TUdpClient client, NetSdrUdpClientSettings settings)
        {
            _client = client;
            _port = settings.Port;
            _ipAddress = _client.IPAddress;
        }

        public async Task<byte[]> ReadPacket(CancellationToken cancellationToken)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(_ipAddress), _port);
            _client.SetEndPoint(endPoint);
            
            using (_client)
            {
                byte[] dataItem = (await _client.ReceiveAsync(cancellationToken)).Buffer;
                return dataItem;
            }
        }
    }

    public class NetSdrUdpClientSettings
    {
        public int Port { get; set; } = 60000;
    }
}
