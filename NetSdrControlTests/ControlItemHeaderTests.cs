using NetSdrControl.Protocol.HelpMessages;
using NetSdrControl.Protocol;

namespace NetSdrControlTests
{
    public class ControlItemHeaderTests
    {
        [Theory]
        [InlineData(32, 32, 32)]
        [InlineData(16, 0, 16)]
        public void TestDecodeMessageLength(byte lowByte, byte highByte, ushort length)
        {
            var header = new ControlItemHeader();
            ushort decodedLength = header.DecodeMessageLength(lowByte, highByte);

            Assert.Equal(decodedLength, length);
        }

        [Theory]
        [InlineData(32, RequestResponseKind.Get, 32, 32)]
        [InlineData(16, RequestResponseKind.Set, 16, 0)]
        public void TestEncodeMessageLength(ushort messageLength, RequestResponseKind kind, byte lowByte, byte highByte)
        {
            var header = new ControlItemHeader();
            (byte encodedLowByte, byte encodedHighByte) = header.EncodeMessageLength(messageLength, kind);

            Assert.Equal(encodedLowByte, lowByte);
            Assert.Equal(encodedHighByte, highByte);
        }

        [Theory]
        [InlineData(new byte[] { 0x08, 0x00, 0x18, 0x00 }, RequestResponseKind.Set, 8, 0x0018)]
        [InlineData(new byte[] { 0x0A, 0x00, 0x20, 0x00 }, RequestResponseKind.Set, 10, 0x0020)]
        
        public void TestGetHeaderBytes(byte[] messageBytes, RequestResponseKind kind, ushort messageLength, ushort controlItemCode)
        {
            var header = new ControlItemHeader();
            byte[] encodedHeaderBytes = header.GetHeaderBytes(messageLength, (byte)kind, controlItemCode);

            Assert.True(encodedHeaderBytes.SequenceEqual(messageBytes));
        }

        [Theory]
        [InlineData(RequestResponseKind.Get, 32)]
        [InlineData(RequestResponseKind.Set, 0)]
        public void TestEncodeMessageType(RequestResponseKind kind, byte highByte)
        {
            var header = new ControlItemHeader();
            byte encodedKind = header.EncodeMessageType(kind, highByte);

            Assert.Equal(encodedKind, highByte);
        }
    }
}
