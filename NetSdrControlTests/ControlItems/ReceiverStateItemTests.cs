using NetSdrControl.ControlItems;
using NetSdrControl.Protocol.HelpMessages;
using NetSdrControl.Protocol;
using NetSdrControl;
using Xunit.Sdk;
using NetSdrControl.DataItems;

namespace NetSdrControlTests.ControlItems
{
    public class ReceiverStateItemTests
    {
        [Theory]
        [InlineData(RequestResponseKind.Set, NetSdrDataMode.ComplexIQBaseBand, NetSdrCaptureMode.x_24, NetSdrCaptureWay.Contiguous, 0, new byte[] { 0x08, 0x00, 0x18, 0x00, 0x80, 0x02, 0x80, 0x00 })]
        public void TestStartIQMessageFormedCorrectly(RequestResponseKind kind, NetSdrDataMode dataMode, NetSdrCaptureMode captureMode, NetSdrCaptureWay captureWay, byte fifoSamplesCount, byte[] messageBytesRepresentation)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var tcpClient = new MockTcpClient();
            tcpClient.SendTimeout = 1000;
            tcpClient.ReceiveTimeout = 1000;
            var client = new NetSdrTcpClient<MockTcpClient>(new NetSdrTcpClientSettings(), tcpClient, bitDecoder, header);
            var currentDataItemsExchanger = new DataExchanger(new NetSdrUdpClient<MockUdpClient>(
                new MockUdpClient(), new NetSdrUdpClientSettings()), new FileSystem(new FileSystemSettings()));
            byte[] result = messageBuilder.BuildSendMessage(kind, new ReceiverStateMessagePayload(true, fifoSamplesCount, dataMode, captureMode, captureWay),
                new ReceiverStateItem(client, messageBuilder, bitDecoder, currentDataItemsExchanger));

            Assert.True(messageBytesRepresentation.SequenceEqual(result));
        }

        [Theory]
        [InlineData(RequestResponseKind.Set, new byte[] { 0x08, 0x00, 0x18, 0x00, 0x00, 0x01, 0x00, 0x00 })]
        public void TestStopIQMessageFormedCorrectly(RequestResponseKind kind, byte[] messageBytesRepresentation)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var tcpClient = new MockTcpClient();
            var client = new NetSdrTcpClient<MockTcpClient>(new NetSdrTcpClientSettings(), tcpClient, bitDecoder, header);
            var currentDataItemsExchanger = new DataExchanger(new NetSdrUdpClient<MockUdpClient>(
                new MockUdpClient(), new NetSdrUdpClientSettings()), new FileSystem(new FileSystemSettings()));
            byte[] result = messageBuilder.BuildSendMessage(kind, new ReceiverStateMessagePayload(false),
                new ReceiverStateItem(client, messageBuilder, bitDecoder, currentDataItemsExchanger));

            Assert.True(messageBytesRepresentation.SequenceEqual(result));
        }

        [Theory]
        [InlineData(new byte[] { 0x08, 0x00, 0x18, 0x00, 0x80, 0x02, 0x80, 0x00 }, true, NetSdrDataMode.ComplexIQBaseBand, NetSdrCaptureMode.x_24, NetSdrCaptureWay.Contiguous)]
        public void TestStartIQMessageResponseFormedCorrectly(byte[] receivedBytes, bool isRun, NetSdrDataMode dataMode, NetSdrCaptureMode captureMode, NetSdrCaptureWay captureWay)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var tcpClient = new MockTcpClient();
            var client = new NetSdrTcpClient<MockTcpClient>(new NetSdrTcpClientSettings(), tcpClient, bitDecoder, header);
            var currentDataItemsExchanger = new DataExchanger(new NetSdrUdpClient<MockUdpClient>(
                new MockUdpClient(), new NetSdrUdpClientSettings()), new FileSystem(new FileSystemSettings()));
            var result = messageBuilder.BuildReceiveMessage(receivedBytes, new ReceiverStateItem(client, messageBuilder, bitDecoder, currentDataItemsExchanger));

            Assert.NotNull(result);
            Assert.Equal(result.IsRun, isRun);
            Assert.Equal(result.DataMode, dataMode);
            Assert.Equal(result.Mode, captureMode);
            Assert.Equal(result.Way, captureWay);
        }

        [Theory]
        [InlineData(new byte[] { 0x08, 0x00, 0x18, 0x00, 0x00, 0x01, 0x00, 0x00 }, false)]
        public void TestStopIQMessageResponseFormedCorrectly(byte[] receivedBytes, bool isRun)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var tcpClient = new MockTcpClient();
            var currentDataItemsExchanger = new DataExchanger(new NetSdrUdpClient<MockUdpClient>(
                new MockUdpClient(), new NetSdrUdpClientSettings()), new FileSystem(new FileSystemSettings()));
            var client = new NetSdrTcpClient<MockTcpClient>(new NetSdrTcpClientSettings(), tcpClient, bitDecoder, header);
            var result = messageBuilder.BuildReceiveMessage(receivedBytes, new ReceiverStateItem(client, messageBuilder, bitDecoder, currentDataItemsExchanger));

            Assert.NotNull(result);
            Assert.Equal(result.IsRun, isRun);
        }

        [Theory]
        [InlineData(NetSdrDataMode.ComplexIQBaseBand, NetSdrCaptureMode.x_24, NetSdrCaptureWay.Contiguous)]
        public async void TestReceiverStateItemStartIQ(NetSdrDataMode dataMode, NetSdrCaptureMode captureMode, NetSdrCaptureWay captureWay)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var tcpClient = new MockTcpClient();
            var client = new NetSdrTcpClient<MockTcpClient>(new NetSdrTcpClientSettings(), tcpClient, bitDecoder, header);

            var currentDataItemsExchanger = new DataExchanger(new NetSdrUdpClient<MockUdpClient>(
                new MockUdpClient(), new NetSdrUdpClientSettings()), new FileSystem(new FileSystemSettings()));
            var receiveStateItem = new ReceiverStateItem(client, messageBuilder, bitDecoder, currentDataItemsExchanger);

            await client.Connect("127.0.0.1");
            bool shouldStart = await receiveStateItem.StartIQTransfer(dataMode, captureMode, captureWay, 0);
            await Task.Delay(2000);
            await receiveStateItem.StopIQTransfer();
            client.Disconnect();

            byte[] samples = receiveStateItem.GetSamples();
            bool samplesWritten = samples.Length > 0;

            Assert.True(shouldStart);
            Assert.True(samplesWritten);
            Assert.NotNull(samples);
        }
    }
}
