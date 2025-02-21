namespace NetSdrControl.DataItems
{
    public class DataExchanger
    {
        private INetSdrUdpClient _udpClient;
        private IFileSystem _fileSystem;
        private CancellationTokenSource _disconnectionSource;

        public DataExchanger(INetSdrUdpClient udpClient, IFileSystem fileSystem)
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
            _disconnectionSource.Cancel();
        }

        public byte[] GetSamples()
        {
            return _fileSystem.ReadFromFile();
        }
    }
}
