using NetSdrControl.ControlItems;
using NetSdrControl.Protocol.HelpMessages;
using NetSdrControl.Protocol;
using NetSdrControl;

namespace NetSdrControlTests.EndToEndTests
{
    public class ReceiverFrequencyControlItemTests
    {
        [Theory]
        [InlineData(FrequencyChannel.Channel_1, 14010000)]
        public async void TestChangeFrequency(FrequencyChannel channel, ulong frequency)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var tcpClient = new FakeTcpClient();
            tcpClient.ConnectTimeMs = 5000;
            var client = new NetSdrHost(new NetSdrTcpClientSettings(), tcpClient, bitDecoder, header);

            await client.Connect("127.0.0.1");
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

            var tcpClient = new FakeTcpClient();
            tcpClient.ConnectTimeMs = 5000;
            var client = new NetSdrHost(new NetSdrTcpClientSettings(), tcpClient, bitDecoder, header);

            await client.Connect("127.0.0.1");
            var controlItem = new ReceiverFrequencyControlItem(client, messageBuilder, bitDecoder);
            ulong? freq = await controlItem.ChangeFrequency(channel, frequency);

            Assert.NotNull(freq);
            Assert.Equal(freq, frequency);
        }
    }
}
