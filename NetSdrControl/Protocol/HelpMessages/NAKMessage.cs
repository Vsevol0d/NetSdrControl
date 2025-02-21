namespace NetSdrControl.Protocol.HelpMessages
{
    public class NAKMessage
    {
        public int Size => 2;
        public byte LowByte => 2; 
        public byte HighByte => 0;

        public bool IsNAK(byte[] incomingBytes)
        {
            if (incomingBytes == null || incomingBytes.Length < 2) return false;

            return incomingBytes[0] == LowByte && incomingBytes[1] == HighByte;
        }

        public void WriteToBuffer(byte[] incomingBytes)
        {
            incomingBytes[0] = LowByte;
            incomingBytes[1] = HighByte;
        }
    }
}
