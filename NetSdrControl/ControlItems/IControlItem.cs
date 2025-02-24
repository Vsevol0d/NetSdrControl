using NetSdrControl.Interfaces;
using NetSdrControl.Protocol;

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
        List<IPropertyBitsMapping> GetPropertyBitsMappings(RequestResponseType messageType, TPayload contextParameters = default, byte[] contextMessageBytes = null);

        /// <summary>
        /// Each particular Control Item can define a set of Property-to-byte mappings according to protocol version or parameters context
        /// </summary>
        /// <param name="contextParameters"></param>
        /// <returns></returns>
        List<PropertyBytesMapping> GetPropertyBytesMappings(RequestResponseType messageType, TPayload contextParameters = default, byte[] contextPayloadBytes = null);

        /// <summary>
        /// Since NetSdr protocol doesn't have some 'Id' fields and in order to keep track which command was sent and received we need to extract data which is unique for messages
        /// </summary>
        /// <returns></returns>
        List<IPropertyBitsMapping> GetIdentityBitsMappings();
    }
}
