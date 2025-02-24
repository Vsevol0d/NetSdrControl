namespace NetSdrControl
{
    /// <summary>
    /// Allows you to create properties mapping in up to 8('ulong' size) bytes range
    /// </summary>
    public class PropertyBytesMapping
    {
        public bool PreserveNotSetIgnoranceOnEncode { get; set; }
        public int ByteIndex { get; set; }
        public int BytesCount { get; set; }
        public Action<object, ulong> PropertySetter { get; set; }
        public Func<object, ulong?> PropertyGetter { get; set; }

        public PropertyBytesMapping(int byteIndex, int bytesCount, Action<object, ulong> propertySetter, Func<object, ulong?> propertyGetter)
        {
            ByteIndex = byteIndex;
            BytesCount = bytesCount;
            PropertySetter = propertySetter;
            PropertyGetter = propertyGetter;
        }
    }
}
