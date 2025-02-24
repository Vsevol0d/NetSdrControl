using NetSdrControl.Interfaces;

namespace NetSdrControl
{
    public class BitDecoder : IBitDecoder
    {
        public void EncodeByte(Span<byte> payload, int byteIndex, byte propertyValueByteRepresentation)
        {
            payload[byteIndex] |= propertyValueByteRepresentation;
        }

        public byte DecodeByteByMask(Span<byte> sourceBytes, int startByteIndex, byte[] bitIndicesToMask)
        {
            uint maskBase = 1;
            uint totalMask = 0;

            for (int i = 0; i < bitIndicesToMask.Length; i++)
            {
                totalMask |= maskBase << bitIndicesToMask[i];
            }

            uint maskedValue = sourceBytes[startByteIndex] & totalMask;
            return (byte)maskedValue;
        }

        public byte GetByteByBitIndices(byte[] bitIndices, byte[] bitPerIndexValues)
        {
            if (bitIndices.Length != bitPerIndexValues.Length)
            {
                throw new Exception("Bit values count must match bit indices count");
            }

            uint maskBase = 1;
            uint totalMask = 0;

            for (int i = 0; i < bitPerIndexValues.Length; i++)
            {
                if (bitPerIndexValues[i] != 0)
                {
                    totalMask |= maskBase << bitIndices[i];
                }
            }
            return (byte)totalMask;
        }

        private byte[] ConvertULongToBytes(ulong propertyValue)
        {
            return BitConverter.GetBytes(propertyValue);
        }

        public void EncodeULong(Span<byte> payloadSpan, int byteIndex, int bytesCount, ulong propertyValue)
        {
            var bytes = ConvertULongToBytes(propertyValue);
            int counter = 0;
            for (int i = byteIndex; i < byteIndex + bytesCount; i++)
            {
                payloadSpan[i] = bytes[counter++];
            }
        }

        public ulong ConvertBytesToULong(Span<byte> parameters, int byteIndex, int bytesCount)
        {
            ReadOnlySpan<byte> payloadSpan = parameters.Slice(byteIndex, bytesCount);
            if (payloadSpan.Length < sizeof(ulong))
            {
                Span<byte> result = stackalloc byte[sizeof(ulong)];
                payloadSpan.CopyTo(result);
                return BitConverter.ToUInt64(result);
            }
            return BitConverter.ToUInt64(payloadSpan);
        }
    }
}
