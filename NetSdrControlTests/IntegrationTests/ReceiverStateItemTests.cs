using NetSdrControl.ControlItems;
using NetSdrControl.Protocol.HelpMessages;
using NetSdrControl.Protocol;
using NetSdrControl;
using NetSdrControl.DataItems;

namespace NetSdrControlTests.EndToEndTests
{
    public class ReceiverStateItemTests
    {
        [Theory]
        [InlineData(NetSdrDataMode.ComplexIQBaseBand, NetSdrCaptureMode.x_24, NetSdrCaptureWay.Contiguous)]
        public async void TestReceiverStateItemStartIQ(NetSdrDataMode dataMode, NetSdrCaptureMode captureMode, NetSdrCaptureWay captureWay)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var tcpClient = new FakeTcpClient();
            var client = new NetSdrHost(new NetSdrTcpClientSettings(), tcpClient, bitDecoder, header);

            var currentDataItemsExchanger = new DataItemsExchanger(new NetSdrUdpClient<FakeUdpClient>(
                new FakeUdpClient(), new NetSdrUdpClientSettings()), new FileSystem(new FileSystemSettings()));
            var receiveStateItem = new ReceiverStateItem(client, messageBuilder, bitDecoder, currentDataItemsExchanger);

            await client.Connect("127.0.0.1");
            bool shouldStart = await receiveStateItem.StartIQTransfer(dataMode, captureMode, captureWay, 0);
            await Task.Delay(2000);
            await receiveStateItem.StopIQTransfer();
            client.Disconnect();

            Assert.True(shouldStart);
        }
    }
}
