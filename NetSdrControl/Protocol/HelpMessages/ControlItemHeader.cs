using NetSdrControl.Protocol.Interfaces;

namespace NetSdrControl.Protocol.HelpMessages
{
    public class ControlItemHeader : IControlItemHeader
    {
        public ushort HeaderSize => 2;             // Only first 2 bytes with Length and Type values
        public ushort HeaderAndCodeSize => 4;      // 2 Header bytes + 2 Control Item Code bytes

        public ushort DecodeMessageCode(byte codeLowByte, byte codeHighByte)
        {
            return BitConverter.ToUInt16([codeLowByte, codeHighByte]);
        }

        public ushort DecodeMessageLength(byte lowByte, byte highByte)
        {
            byte mask = 0x1F;
            highByte &= mask;
            return BitConverter.ToUInt16([lowByte, highByte], 0);
        }

        public (byte lowByte, byte highByte) EncodeMessageLength(ushort messageLength, RequestResponseType kind)
        {
            byte lowLengthByte = (byte)(messageLength & 0xFF);
            byte highLengthByte = (byte)(messageLength >> 8);
            highLengthByte &= 0x1F;
            byte highByte = EncodeMessageType(kind, highLengthByte);
            return (lowLengthByte, highByte);
        }

        public byte EncodeMessageType(RequestResponseType kind, byte highHeaderByte)
        {
            byte highByte = (byte)(highHeaderByte | ((byte)kind << 5));      // Apply message Type field
            return highByte;
        }

        public byte[] GetHeaderBytes(ushort length, byte kind, ushort controlItemCode)
        {
            var headerBytes = new byte[HeaderAndCodeSize];
            (byte lowByte, byte highByte) = EncodeMessageLength(length, (RequestResponseType)kind);
            headerBytes[0] = lowByte;
            headerBytes[1] = highByte;
            headerBytes[2] = (byte)(controlItemCode & 0xFF);            // Write low byte first
            headerBytes[3] = (byte)(controlItemCode >> 8);              // Write high byte next
            return headerBytes;
        }
    }
}
