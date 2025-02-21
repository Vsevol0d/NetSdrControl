using NetSdrControl;
using NetSdrControl.ControlItems;

namespace NetSdrControlTests
{
    public class BitMapperTests
    {
        [Theory]
        [InlineData(new byte[] { 7 }, new byte[] { 0 }, 0)]
        [InlineData(new byte[] { 3 }, new byte[] { 1 }, 8)]
        [InlineData(new byte[] { 0, 1, 2 }, new byte[] { 1, 1, 1 }, 7)]
        [InlineData(new byte[] { 0, 1, 2, 3 }, new byte[] { 1, 0, 1, 0 }, 5)]
        [InlineData(new byte[] { 3, 1, 2, 0 }, new byte[] { 0, 0, 1, 1 }, 5)]
        [InlineData(new byte[] { 1, 0, 3, 2 }, new byte[] { 0, 1, 0, 1 }, 5)]
        public void TestBitIndicesAndPossibleValuesSequenceMatch(byte[] indices, byte[] possibleValueIndices, byte value)
        {
            byte[] sourceBytes = [255, 255, 255, 255];
            var bitDecoder = new BitDecoder();

            var testMapping = new PropertyBitsMapping<ReceiverStateMessagePayload, NetSdrDataMode?>(bitDecoder, 0, indices,
                (payload, dataMode) => { payload.DataMode = dataMode; }, (payload) => { return payload.DataMode; }, [(possibleValueIndices, NetSdrDataMode.RealADSample)]);

            byte mappedByte = testMapping.PossibleValuesToByteMap[NetSdrDataMode.RealADSample];

            Assert.Equal(value, mappedByte);
        }

        [Theory]
        [InlineData(new byte[] { 1 }, new byte[] { 1, 0 }, BitDecoderException.IndicesCountMismatch)]
        [InlineData(new byte[] { 1, 2 }, new byte[] { 0 }, BitDecoderException.IndicesCountMismatch)]
        [InlineData(new byte[] { 0, 1, 15 }, new byte[] { 1, 1, 1 }, BitDecoderException.ByteIndexOutOfRange)]
        [InlineData(new byte[] { 3 }, new byte[] { 5 }, BitDecoderException.BitValueOutOfRange)]
        public void TestBitDecoderEdgeCasesHandledCorrectly(byte[] indices, byte[] possibleValueIndices, string exceptionMessage)
        {
            byte[] sourceBytes = [255, 255, 255, 255];
            var bitDecoder = new BitDecoder();

            try
            {
                var testMapping = new PropertyBitsMapping<ReceiverStateMessagePayload, NetSdrDataMode?>(bitDecoder, 0, indices,
                (payload, dataMode) => { payload.DataMode = dataMode; }, (payload) => { return payload.DataMode; }, [(possibleValueIndices, NetSdrDataMode.RealADSample)]);
            }
            catch (BitDecoderException ex)
            {
                Assert.Equal(exceptionMessage, ex.Message);
            }
        }
    }
}
