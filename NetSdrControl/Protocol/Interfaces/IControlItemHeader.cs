namespace NetSdrControl.Protocol.Interfaces
{
    public interface IControlItemHeader
    {
        ushort HeaderSize { get; }
        ushort HeaderAndCodeSize { get; }
        ushort DecodeMessageLength(byte lowByte, byte highByte);
        ushort DecodeMessageCode(byte lowByte, byte highByte);
        byte[] GetHeaderBytes(ushort length, byte kind, ushort controlItemCode);
        (byte lowByte, byte highByte) EncodeMessageLength(ushort payloadLength, RequestResponseType kind);
        byte EncodeMessageType(RequestResponseType kind, byte highHeaderByte);
    }
}
