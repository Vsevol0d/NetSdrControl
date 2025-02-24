namespace NetSdrControl.Interfaces
{
    public interface IBitDecoder
    {
        void EncodeByte(Span<byte> payload, int byteIndex, byte obj);
        byte DecodeByteByMask(Span<byte> sourceBytes, int startByteIndex, byte[] bitIndices);
        byte GetByteByBitIndices(byte[] bitIndices, byte[] item1);
        void EncodeULong(Span<byte> payloadSpan, int byteIndex, int bytesCount, ulong propertyValue);
        ulong ConvertBytesToULong(Span<byte> parameters, int byteIndex, int bytesCount);
    }
}
