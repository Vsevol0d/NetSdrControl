namespace NetSdrControl.DataItems
{
    public interface IDataItemsExchanger
    {
        void StartWriting();
        void StopWriting();
        byte[] GetSamples();
    }

    public class DataItemsExchanger : IDataItemsExchanger
    {
        private INetSdrUdpClient _udpClient;
        private IFileSystem _fileSystem;
        private CancellationTokenSource _disconnectionSource;

        public DataItemsExchanger(INetSdrUdpClient udpClient, IFileSystem fileSystem)
        {
            _udpClient = udpClient;
            _fileSystem = fileSystem;
            _disconnectionSource = new CancellationTokenSource();
        }

        public void StartWriting()
        {
            Task.Run(async () =>
            {
                while (!_disconnectionSource.IsCancellationRequested)
                {
                    byte[] rawArr = await _udpClient.ReadPacket(_disconnectionSource.Token);
                    if (rawArr.Length < 2)
                    {
                        continue;
                    }

                    if (rawArr[1] == 0x84)
                    {
                        WriteLargeMTUPacket(rawArr);
                    }
                    else if (rawArr[1] == 0x82)
                    {
                        WriteSmallMTUPacket(rawArr);
                    }
                }
            });
        }

        private void WriteSmallMTUPacket(Span<byte> rawArr)
        {
            var sampleBytes = rawArr.Slice(4, 512);
            _fileSystem.WriteToFile(sampleBytes);
        }

        private void WriteLargeMTUPacket(Span<byte> rawArr)
        {
            var sampleBytes = rawArr.Slice(4, 1024);
            _fileSystem.WriteToFile(sampleBytes);
        }
        public void StopWriting()
        {
            _fileSystem.CloseFile();
            _disconnectionSource.Cancel();
        }

        public byte[] GetSamples()
        {
            return _fileSystem.ReadFromFile();
        }
    }
}
