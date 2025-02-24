namespace NetSdrControl.Protocol.Interfaces
{
    public interface INetSdrHost
    {
        string IpAddress { get; }

        Task<bool> Connect(string ipAddress);
        Task Disconnect();
        Task<byte[]> Send(byte[] message, (int StartByteOffset, byte[] BitsIndices)[] identifierBits);
    }
}
