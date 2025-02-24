namespace NetSdrControl.DataItems
{
    public interface IFileSystem
    {
        void CloseFile();
        byte[] ReadFromFile();
        void WriteToFile(ReadOnlySpan<byte> packetSamples);
    }
}
