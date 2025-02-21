namespace NetSdrControl.DataItems
{
    public class FileSystem : IFileSystem
    {
        private FileStream _fileStream;
        private string _filePath;

        public FileSystem(FileSystemSettings settings)
        {
            _filePath = settings.FilePath;
            _fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write);
        }

        public void WriteToFile(ReadOnlySpan<byte> packetSamples)
        {
            try
            {
                _fileStream.Write(packetSamples);
            }
            catch (Exception ex)
            {
                StopWriting();
                Console.WriteLine("Packet write failed. Reason: " + ex.Message);
            }
        }
        public void StartWriting()
        {
            _fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write);
        }
        public void StopWriting()
        {
            _fileStream.Close();
            _fileStream.Dispose();
            _fileStream = null;
        }

        public byte[] ReadFromFile()
        {
            return File.ReadAllBytes(_fileStream.Name);
        }
    }

    public class FileSystemSettings
    {
        public string FilePath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "Samples.dat");
    }
}
