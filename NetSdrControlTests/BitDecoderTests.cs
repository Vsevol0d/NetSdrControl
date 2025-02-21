using NetSdrControl;

namespace NetSdrControlTests
{
    public class BitDecoderTests
    {
        [Theory]
        [InlineData(new byte[] { 0, 1, 2 }, 255, 7)]
        [InlineData(new byte[] { 0, 1 }, 252, 0)]
        [InlineData(new byte[] { 1, 2, 3 }, 251, 10)]
        public void TestMaskedByteFormedCorrectly(byte[] maskedBitIndices, byte originalByte, byte maskedValue)
        {
            var bitDecoder = new BitDecoder();

            byte result = bitDecoder.DecodeByteByMask([originalByte], 0, maskedBitIndices);

            Assert.Equal(maskedValue, result);
        }

        [Theory]
        [InlineData(new byte[] { 255, 251 }, 1, 4, 255)]
        [InlineData(new byte[] { 255, 200 }, 0, 0, 255)]
        public void TestByteEncodedCorrectly(byte[] bytes, int byteWritingIndex, byte byteToWrite, byte resultingByteValue)
        {
            var bitDecoder = new BitDecoder();

            bitDecoder.EncodeByte(bytes, byteWritingIndex, byteToWrite);

            Assert.Equal(bytes[byteWritingIndex], resultingByteValue);
        }

        [Theory]
        [InlineData(new byte[] { 255, 1, 16, 2, 68 }, 2, 2, 528)]
        [InlineData(new byte[] { 255, 1, 1, 1, 68 }, 1, 3, 65793)]
        public void TestBytesConvertedToULongCorrectly(byte[] bytes, int byteIndex, int bytesCount, ulong convertedValue)
        {
            var bitDecoder = new BitDecoder();

            ulong result = bitDecoder.ConvertBytesToULong(bytes, byteIndex, bytesCount);

            Assert.Equal(convertedValue, result);
        }
    }
}