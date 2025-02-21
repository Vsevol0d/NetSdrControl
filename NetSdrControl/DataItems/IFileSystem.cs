namespace NetSdrControl.DataItems
{
    public interface IFileSystem
    {
        byte[] ReadFromFile();
        void WriteToFile(ReadOnlySpan<byte> packetSamples);
    }
}
