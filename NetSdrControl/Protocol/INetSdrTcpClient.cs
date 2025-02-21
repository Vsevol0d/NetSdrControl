namespace NetSdrControl.Protocol
{
    public interface INetSdrTcpClient
    {
        string IpAddress { get; }

        Task<bool> Connect(string ipAddress);
        void Disconnect();
        Task<byte[]> Send(byte[] message);
    }
}
