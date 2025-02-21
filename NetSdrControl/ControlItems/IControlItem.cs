namespace NetSdrControl.ControlItems
{
    public interface IControlItem<TPayload>
    {
        /// <summary>
        /// Control Item's predefined code
        /// </summary>
        ushort Code { get; }

        /// <summary>
        /// Each particular Control Item can define a set of Property-to-bit mappings according to protocol version or parameters context
        /// </summary>
        /// <param name="contextParameters"></param>
        /// <returns></returns>
        List<IPropertyBitsMapping> GetPropertyBitsMappings(TPayload contextParameters = default, byte[] contextMessageBytes = null);

        /// <summary>
        /// Each particular Control Item can define a set of Property-to-byte mappings according to protocol version or parameters context
        /// </summary>
        /// <param name="contextParameters"></param>
        /// <returns></returns>
        List<PropertyBytesMapping> GetPropertyBytesMappings(TPayload contextParameters = default, byte[] contextPayloadBytes = null);
    }
}
