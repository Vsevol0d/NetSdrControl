namespace NetSdrControl.Interfaces
{
    public interface IPropertyBitsMapping
    {
        /// <summary>
        /// MessageBuilder by default ignores all properties which were not set. This flag forces MessageBuilder to not ignore unset property
        /// </summary>
        bool PreserveNotSetIgnoranceOnEncode { get; set; }

        /// <summary>
        /// Byte in message bytes sequence at which bits mapping is defined
        /// </summary>
        int StartByteIndex { get; set; }

        /// <summary>
        /// Indices of bits which corresponds to the mapped property
        /// </summary>
        byte[] BitIndices { get; set; }

        /// <summary>
        /// Encoding bits to possible property values map
        /// </summary>
        Dictionary<byte, object> PossibleValuesMap { get; set; }

        /// <summary>
        /// Possible property values to encoding bits map
        /// </summary>
        Dictionary<object, byte> PossibleValuesToByteMap { get; set; }

        /// <summary>
        /// Property setter method. Called by MessageBuilder to decode(read) property value from message's byte array
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="value"></param>
        public void SetProperty(object payload, object value);

        /// <summary>
        /// Property setter method. Called by MessageBuilder to encode(write) property value into message's byte array
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public object? GetProperty(object payload);
    }
}
