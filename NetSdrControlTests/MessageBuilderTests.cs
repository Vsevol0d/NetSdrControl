using NetSdrControl.ControlItems;
using NetSdrControl.Protocol.HelpMessages;
using NetSdrControl.Protocol;
using NetSdrControl;
using NetSdrControl.DataItems;
using Moq;
using NetSdrControl.Protocol.Interfaces;

namespace NetSdrControlTests
{
    public class MessageBuilderTests
    {
        [Theory]
        [InlineData(RequestResponseType.Get, FrequencyChannel.Channel_2, new byte[] { 0x05, 0x20, 0x20, 0x00, 0x02 })]
        public void TestGetMessageFormedCorrectly(RequestResponseType kind, FrequencyChannel channel, byte[] messageBytesRepresentation)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var netSdrClientMock = new Mock<INetSdrHost>();
            netSdrClientMock.Setup(x => x.IpAddress).Returns(It.IsAny<string>());
            netSdrClientMock.Setup(x => x.Connect(It.IsAny<string>())).Returns(Task.FromResult(true));
            netSdrClientMock.Setup(x => x.Disconnect());
            netSdrClientMock.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<(int StartByteOffset, byte[] BitsIndices)[]>())).Returns(Task.FromResult(new byte[0]));

            byte[] result = messageBuilder.BuildSendMessage(kind, new ReceiverFrequencyMessagePayload(channel),
                new ReceiverFrequencyControlItem(netSdrClientMock.Object, messageBuilder, bitDecoder));

            Assert.True(messageBytesRepresentation.SequenceEqual(result));
        }

        [Theory]
        [InlineData(RequestResponseType.Set, FrequencyChannel.Channel_1, 14010000, new byte[] { 0x0A, 0x00, 0x20, 0x00, 0x00, 0x90, 0xC6, 0xD5, 0x00, 0x00 })]
        public void TestSetMessageFormedCorrectly(RequestResponseType kind, FrequencyChannel channel, ulong frequency, byte[] messageBytesRepresentation)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var netSdrClientMock = new Mock<INetSdrHost>();
            netSdrClientMock.Setup(x => x.IpAddress).Returns(It.IsAny<string>());
            netSdrClientMock.Setup(x => x.Connect(It.IsAny<string>())).Returns(Task.FromResult(true));
            netSdrClientMock.Setup(x => x.Disconnect());
            netSdrClientMock.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<(int StartByteOffset, byte[] BitsIndices)[]>())).Returns(Task.FromResult(new byte[0]));

            byte[] result = messageBuilder.BuildSendMessage(kind, new ReceiverFrequencyMessagePayload(channel, frequency),
                new ReceiverFrequencyControlItem(netSdrClientMock.Object, messageBuilder, bitDecoder));

            Assert.True(messageBytesRepresentation.SequenceEqual(result));
        }

        [Theory]
        [InlineData(new byte[] { 0x0A, 0x00, 0x20, 0x00, 0x02, 0x90, 0xC6, 0xD5, 0x00, 0x00 }, FrequencyChannel.Channel_2, 14010000)]
        public void TestGetMessageResponseFormedCorrectly(byte[] receivedBytes, FrequencyChannel channel, ulong frequency)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var netSdrClientMock = new Mock<INetSdrHost>();
            netSdrClientMock.Setup(x => x.IpAddress).Returns(It.IsAny<string>());
            netSdrClientMock.Setup(x => x.Connect(It.IsAny<string>())).Returns(Task.FromResult(true));
            netSdrClientMock.Setup(x => x.Disconnect());
            netSdrClientMock.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<(int StartByteOffset, byte[] BitsIndices)[]>())).Returns(Task.FromResult(new byte[0]));

            var result = messageBuilder.BuildReceiveMessage(RequestResponseType.Get, receivedBytes, new ReceiverFrequencyControlItem(
                netSdrClientMock.Object, messageBuilder, bitDecoder));

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

            var netSdrClientMock = new Mock<INetSdrHost>();
            netSdrClientMock.Setup(x => x.IpAddress).Returns(It.IsAny<string>());
            netSdrClientMock.Setup(x => x.Connect(It.IsAny<string>())).Returns(Task.FromResult(true));
            netSdrClientMock.Setup(x => x.Disconnect());
            netSdrClientMock.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<(int StartByteOffset, byte[] BitsIndices)[]>())).Returns(Task.FromResult(new byte[0]));

            var result = messageBuilder.BuildReceiveMessage(RequestResponseType.Set, receivedBytes, new ReceiverFrequencyControlItem(
                netSdrClientMock.Object, messageBuilder, bitDecoder));

            Assert.NotNull(result);
            Assert.Equal(result.FrequencyChannel, channel);
            Assert.Equal(result.CenterFrequencyValue, frequency);
        }




        [Theory]
        [InlineData(RequestResponseType.Set, NetSdrDataMode.ComplexIQBaseBand, NetSdrCaptureMode.x_24, NetSdrCaptureWay.Contiguous, 0, new byte[] { 0x08, 0x00, 0x18, 0x00, 0x80, 0x02, 0x80, 0x00 })]
        public void TestStartIQMessageFormedCorrectly(RequestResponseType kind, NetSdrDataMode dataMode, NetSdrCaptureMode captureMode, NetSdrCaptureWay captureWay, byte fifoSamplesCount, byte[] messageBytesRepresentation)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var dataItemsExchangeMock = new Mock<IDataItemsExchanger>();
            dataItemsExchangeMock.Setup(x => x.StartWriting());
            dataItemsExchangeMock.Setup(x => x.StopWriting());
            dataItemsExchangeMock.Setup(x => x.GetSamples()).Returns([]);

            var netSdrClientMock = new Mock<INetSdrHost>();
            netSdrClientMock.Setup(x => x.IpAddress).Returns(It.IsAny<string>());
            netSdrClientMock.Setup(x => x.Connect(It.IsAny<string>())).Returns(Task.FromResult(true));
            netSdrClientMock.Setup(x => x.Disconnect());
            netSdrClientMock.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<(int StartByteOffset, byte[] BitsIndices)[]>())).Returns(Task.FromResult(new byte[0]));

            byte[] result = messageBuilder.BuildSendMessage(kind, new ReceiverStateMessagePayload(true, fifoSamplesCount, dataMode, captureMode, captureWay),
                new ReceiverStateItem(netSdrClientMock.Object, messageBuilder, bitDecoder, dataItemsExchangeMock.Object));

            Assert.True(messageBytesRepresentation.SequenceEqual(result));
        }

        [Theory]
        [InlineData(RequestResponseType.Set, new byte[] { 0x08, 0x00, 0x18, 0x00, 0x00, 0x01, 0x00, 0x00 })]
        public void TestStopIQMessageFormedCorrectly(RequestResponseType kind, byte[] messageBytesRepresentation)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var dataItemsExchangeMock = new Mock<IDataItemsExchanger>();
            dataItemsExchangeMock.Setup(x => x.StartWriting());
            dataItemsExchangeMock.Setup(x => x.StopWriting());
            dataItemsExchangeMock.Setup(x => x.GetSamples()).Returns([]);

            var netSdrClientMock = new Mock<INetSdrHost>();
            netSdrClientMock.Setup(x => x.IpAddress).Returns(It.IsAny<string>());
            netSdrClientMock.Setup(x => x.Connect(It.IsAny<string>())).Returns(Task.FromResult(true));
            netSdrClientMock.Setup(x => x.Disconnect());
            netSdrClientMock.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<(int StartByteOffset, byte[] BitsIndices)[]>())).Returns(Task.FromResult(new byte[0]));

            byte[] result = messageBuilder.BuildSendMessage(kind, new ReceiverStateMessagePayload(false),
                new ReceiverStateItem(netSdrClientMock.Object, messageBuilder, bitDecoder, dataItemsExchangeMock.Object));

            Assert.True(messageBytesRepresentation.SequenceEqual(result));
        }

        [Theory]
        [InlineData(new byte[] { 0x08, 0x00, 0x18, 0x00, 0x80, 0x02, 0x80, 0x00 }, true, NetSdrDataMode.ComplexIQBaseBand, NetSdrCaptureMode.x_24, NetSdrCaptureWay.Contiguous)]
        public void TestStartIQMessageResponseFormedCorrectly(byte[] receivedBytes, bool isRun, NetSdrDataMode dataMode, NetSdrCaptureMode captureMode, NetSdrCaptureWay captureWay)
        {
            var bitDecoder = new BitDecoder();
            var header = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, header, new NAKMessage());

            var dataItemsExchangeMock = new Mock<IDataItemsExchanger>();
            dataItemsExchangeMock.Setup(x => x.StartWriting());
            dataItemsExchangeMock.Setup(x => x.StopWriting());
            dataItemsExchangeMock.Setup(x => x.GetSamples()).Returns([]);

            var netSdrClientMock = new Mock<INetSdrHost>();
            netSdrClientMock.Setup(x => x.IpAddress).Returns(It.IsAny<string>());
            netSdrClientMock.Setup(x => x.Connect(It.IsAny<string>())).Returns(Task.FromResult(true));
            netSdrClientMock.Setup(x => x.Disconnect());
            netSdrClientMock.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<(int StartByteOffset, byte[] BitsIndices)[]>())).Returns(Task.FromResult(new byte[0]));

            var receiverControlItem = new ReceiverStateItem(netSdrClientMock.Object, messageBuilder, bitDecoder, dataItemsExchangeMock.Object);

            var result = messageBuilder.BuildReceiveMessage(RequestResponseType.Set, receivedBytes, receiverControlItem);

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

            var dataItemsExchangeMock = new Mock<IDataItemsExchanger>();
            dataItemsExchangeMock.Setup(x => x.StartWriting());
            dataItemsExchangeMock.Setup(x => x.StopWriting());
            dataItemsExchangeMock.Setup(x => x.GetSamples()).Returns([]);

            var netSdrClientMock = new Mock<INetSdrHost>();
            netSdrClientMock.Setup(x => x.IpAddress).Returns(It.IsAny<string>());
            netSdrClientMock.Setup(x => x.Connect(It.IsAny<string>())).Returns(Task.FromResult(true));
            netSdrClientMock.Setup(x => x.Disconnect());
            netSdrClientMock.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<(int StartByteOffset, byte[] BitsIndices)[]>())).Returns(Task.FromResult(new byte[0]));

            var result = messageBuilder.BuildReceiveMessage(RequestResponseType.Set, receivedBytes, new ReceiverStateItem(netSdrClientMock.Object, 
                messageBuilder, bitDecoder, dataItemsExchangeMock.Object));

            Assert.NotNull(result);
            Assert.Equal(result.IsRun, isRun);
        }
    }
}
