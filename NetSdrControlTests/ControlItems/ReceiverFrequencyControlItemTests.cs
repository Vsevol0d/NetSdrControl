using NetSdrControl.ControlItems;
using NetSdrControl.Protocol.HelpMessages;
using NetSdrControl.Protocol;
using NetSdrControl;

namespace NetSdrControlTests.ControlItems
{
    public class ReceiverFrequencyControlItemTests
    {
        [Theory]
        [InlineData(RequestResponseKind.Get, FrequencyChannel.Channel_2, new byte[] { 0x05, 0x20, 0x20, 0x00, 0x02 })]
        public void TestGetMessageFormedCorrectly(RequestResponseKind kind, FrequencyChannel channel, byte[] messageBytesRepresentation)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var tcpClient = new MockTcpClient();
            var client = new NetSdrTcpClient<MockTcpClient>(new NetSdrTcpClientSettings(), tcpClient, bitDecoder, header);
            byte[] result = messageBuilder.BuildSendMessage(kind, new ReceiverFrequencyMessagePayload(channel),
                new ReceiverFrequencyControlItem(client, messageBuilder, bitDecoder));

            Assert.True(messageBytesRepresentation.SequenceEqual(result));
        }

        [Theory]
        [InlineData(RequestResponseKind.Set, FrequencyChannel.Channel_1, 14010000, new byte[] { 0x0A, 0x00, 0x20, 0x00, 0x00, 0x90, 0xC6, 0xD5, 0x00, 0x00 })]
        public void TestSetMessageFormedCorrectly(RequestResponseKind kind, FrequencyChannel channel, ulong frequency, byte[] messageBytesRepresentation)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var tcpClient = new MockTcpClient();
            var client = new NetSdrTcpClient<MockTcpClient>(new NetSdrTcpClientSettings(), tcpClient, bitDecoder, header);
            byte[] result = messageBuilder.BuildSendMessage(kind, new ReceiverFrequencyMessagePayload(channel, frequency),
                new ReceiverFrequencyControlItem(client, messageBuilder, bitDecoder));

            Assert.True(messageBytesRepresentation.SequenceEqual(result));
        }

        [Theory]
        [InlineData(new byte[] { 0x0A, 0x00, 0x20, 0x00, 0x02, 0x90, 0xC6, 0xD5, 0x00, 0x00 }, FrequencyChannel.Channel_2, 14010000)]
        public void TestGetMessageResponseFormedCorrectly(byte[] receivedBytes, FrequencyChannel channel, ulong frequency)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var tcpClient = new MockTcpClient();
            var client = new NetSdrTcpClient<MockTcpClient>(new NetSdrTcpClientSettings(), tcpClient, bitDecoder, header);
            var result = messageBuilder.BuildReceiveMessage(receivedBytes, new ReceiverFrequencyControlItem(client, messageBuilder, bitDecoder));

            Assert.NotNull(result);
            Assert.Equal(result.FrequencyChannel, channel);
            Assert.Equal(result.CenterFrequencyValue, frequency);
        }

        [Theory]
        [InlineData(new byte[] { 0x0A, 0x00, 0x20, 0x00, 0x00, 0x90, 0xC6, 0xD5, 0x00, 0x00 }, FrequencyChannel.Channel_1, 14010000)]
        public void TestSetMessageResponseFormedCorrectly(byte[] receivedBytes, FrequencyChannel channel, ulong frequency)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var tcpClient = new MockTcpClient();
            var client = new NetSdrTcpClient<MockTcpClient>(new NetSdrTcpClientSettings(), tcpClient, bitDecoder, header);
            var result = messageBuilder.BuildReceiveMessage(receivedBytes, new ReceiverFrequencyControlItem(client, messageBuilder, bitDecoder));

            Assert.NotNull(result);
            Assert.Equal(result.FrequencyChannel, channel);
            Assert.Equal(result.CenterFrequencyValue, frequency);
        }

        [Theory]
        [InlineData(FrequencyChannel.Channel_1, 14010000)]
        public async void TestChangeFrequency(FrequencyChannel channel, ulong frequency)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var tcpClient = new MockTcpClient();
            var client = new NetSdrTcpClient<MockTcpClient>(new NetSdrTcpClientSettings(), tcpClient, bitDecoder, header);

            var controlItem = new ReceiverFrequencyControlItem(client, messageBuilder, bitDecoder);
            ulong? freq = await controlItem.ChangeFrequency(channel, frequency);

            Assert.NotNull(freq);
            Assert.Equal(freq, frequency);
        }

        [Theory]
        [InlineData(FrequencyChannel.Channel_1, 14010000)]
        public async void TestGetFrequency(FrequencyChannel channel, ulong frequency)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var tcpClient = new MockTcpClient();
            var client = new NetSdrTcpClient<MockTcpClient>(new NetSdrTcpClientSettings(), tcpClient, bitDecoder, header);

            var controlItem = new ReceiverFrequencyControlItem(client, messageBuilder, bitDecoder);
            ulong? freq = await controlItem.ChangeFrequency(channel, frequency);

            Assert.NotNull(freq);
            Assert.Equal(freq, frequency);
        }
    }
}
