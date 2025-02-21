namespace NetSdrControl
{
    public interface IPropertyBitsMapping
    {
        int StartByteIndex { get; set; }
        byte[] BitIndices { get; set; }
        Dictionary<byte, object> PossibleValuesMap { get; set; }
        Dictionary<object, byte> PossibleValuesToByteMap { get; set; }
        public void SetProperty(object payload, object value);
        public object? GetProperty(object payload);
    }

    public class PropertyBitsMapping<TPayload, TPropertyMapValue> : IPropertyBitsMapping
    {
        public int StartByteIndex { get; set; }
        public byte[] BitIndices { get; set; }
        public Func<TPayload, TPropertyMapValue?> PropertyGetter { get; set; }
        public Action<TPayload, TPropertyMapValue> PropertySetter { get; set; }
        public Dictionary<byte, object> PossibleValuesMap { get; set; }
        public Dictionary<object, byte> PossibleValuesToByteMap { get; set; }

        private IBitDecoder _bitDecoder;

        public PropertyBitsMapping(IBitDecoder bitDecoder, int startByteIndex, byte[] bitIndices, Action<TPayload, TPropertyMapValue> propertySetter,
            Func<TPayload, TPropertyMapValue?> propertyGetter, (byte[], object)[] possibleValues)
        {
            _bitDecoder = bitDecoder;
            StartByteIndex = startByteIndex;
            BitIndices = bitIndices;
            PropertyGetter = propertyGetter;
            PossibleValuesMap = new Dictionary<byte, object>();
            PossibleValuesToByteMap = new Dictionary<object, byte>();

            if (bitIndices.Any(index => index > 7))
            {
                throw new BitDecoderException(BitDecoderException.ByteIndexOutOfRange);
            }

            foreach (var possibleValue in possibleValues)
            {
                if (possibleValue.Item1.Any(index => index > 1))
                {
                    throw new BitDecoderException(BitDecoderException.BitValueOutOfRange);
                }
                if (possibleValue.Item1.Length != bitIndices.Length)
                {
                    throw new BitDecoderException(BitDecoderException.IndicesCountMismatch);
                }

                byte val = _bitDecoder.GetByteByBitIndices(BitIndices, possibleValue.Item1);
                PossibleValuesMap[val] = possibleValue.Item2;
                PossibleValuesToByteMap[possibleValue.Item2] = val;
            }
            PropertySetter = propertySetter;
        }

        public void SetProperty(object payload, object value)
        {
            PropertySetter((TPayload)payload, (TPropertyMapValue)value);
        }

        public object? GetProperty(object payload)
        {
            return PropertyGetter((TPayload)payload);
        }
    }

    public class BitDecoderException : Exception
    {
        public const string IndicesCountMismatch = "IndicesCountMismatch";
        public const string BitValueOutOfRange = "BitValueOutOfRange";
        public const string ByteIndexOutOfRange = "ByteIndexOutOfRange";

        public BitDecoderException(string exceptionMessage) : base(exceptionMessage)
        {
        }
    }
}
