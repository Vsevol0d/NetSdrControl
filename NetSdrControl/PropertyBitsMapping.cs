using NetSdrControl.Interfaces;

namespace NetSdrControl
{
    /// <summary>
    /// Allows you to create properties mapping in up to 8('byte' size) bits range
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    /// <typeparam name="TPropertyMapValue"></typeparam>
    public class PropertyBitsMapping<TPayload, TPropertyMapValue> : IPropertyBitsMapping
    {
        public bool PreserveNotSetIgnoranceOnEncode { get; set; } = false;
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
