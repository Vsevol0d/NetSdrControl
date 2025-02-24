using Moq;
using NetSdrControl;
using NetSdrControl.Protocol;
using NetSdrControl.Protocol.HelpMessages;

namespace NetSdrControlTests
{
    public class NetSdrTcpClientTests
    {
        [Fact]
        public async void TestLongRunningRequestInterruptsAfterTimeout()
        {
            var fakeTcpClient = new FakeTcpClient();
            fakeTcpClient.ConnectTimeMs = 2000;
            var client = new NetSdrHost(new NetSdrTcpClientSettings(), fakeTcpClient, new BitDecoder(), new ControlItemHeader());

            await client.Connect("127.0.0.1");
        }

        [Fact]
        public async void TestFailedRequestHandledCorrectly()
        {
            var fakeTcpClient = new FakeTcpClient();
            fakeTcpClient.ConnectTimeMs = 2000;
            var client = new NetSdrHost(new NetSdrTcpClientSettings(), fakeTcpClient, new BitDecoder(), new ControlItemHeader());

            await client.Connect("127.0.0.1");
        }

        [Fact]
        public async void TestCommandsNotExecutedUntilConnectionEstablished()
        {
            var fakeTcpClient = new FakeTcpClient();
            fakeTcpClient.ConnectTimeMs = 2000;
            var client = new NetSdrHost(new NetSdrTcpClientSettings(), fakeTcpClient, new BitDecoder(), new ControlItemHeader());

            try
            {
                await client.Send([1, 2, 3], [(1, [1])]);
            }
            catch (Exception ex)
            {
                Assert.Equal(NetSdrHostException.SendException, ex.Message);
            }
        }

        [Fact]
        public async void TestReconnectAfterDisconnectFailed()
        {
            var fakeTcpClient = new FakeTcpClient();
            fakeTcpClient.ConnectTimeMs = 2000;
            var client = new NetSdrHost(new NetSdrTcpClientSettings(), fakeTcpClient, new BitDecoder(), new ControlItemHeader());

            await client.Connect("127.0.0.1");
            await client.Disconnect();

            bool connectedSecondTime = await client.Connect("127.0.0.1");

            Assert.True(connectedSecondTime);
        }

        [Fact]
        public async void TestDisconnectSuccessful()
        {
            var fakeTcpClient = new FakeTcpClient();
            fakeTcpClient.ConnectTimeMs = 2000;
            var client = new NetSdrHost(new NetSdrTcpClientSettings(), fakeTcpClient, new BitDecoder(), new ControlItemHeader());

            await client.Connect("127.0.0.1");
            await client.Disconnect();
        }

        [Fact]
        public async void TestSingleConnectionIsBeingEstablishedAtTime()
        {
            var fakeTcpClient = new Mock<FakeTcpClient>() { CallBase = true };
            fakeTcpClient.Object.ConnectTimeMs = 2000;
            var mockService = new Mock<NetSdrHost>(new NetSdrTcpClientSettings(), fakeTcpClient.Object, new BitDecoder(), new ControlItemHeader()) { CallBase = true };

            mockService.Object.Connect("127.0.0.1");
            mockService.Object.Connect("127.0.0.1");
            await mockService.Object.Connect("127.0.0.1");

            int getValueCount = fakeTcpClient.Invocations.Count(i => i.Method.Name == nameof(FakeTcpClient.ConnectAsync));

            Assert.Equal(1, getValueCount);
        }
    }
}
