using NetSdrControl.ControlItems;
using NetSdrControl.Interfaces;
using NetSdrControl.Protocol.HelpMessages;
using NetSdrControl.Protocol.Interfaces;

namespace NetSdrControl.Protocol
{
    public class MessageBuilder : IMessageBuilder
    {
        private readonly IBitDecoder _bitDecoder;
        private readonly IControlItemHeader _controlItemHeader;
        private readonly NAKMessage _nakMessage;

        public MessageBuilder(IBitDecoder bitDecoder, IControlItemHeader controlItemHeader, NAKMessage nakMessage)
        {
            _bitDecoder = bitDecoder;
            _nakMessage = nakMessage;
            _controlItemHeader = controlItemHeader;
        }

        private int CalculatePayloadSize<TPayload>(RequestResponseType type, TPayload payloadParameters, IControlItem<TPayload> controlItem)
        {
            int maxIndex = 0;
            var bitMappings = controlItem.GetPropertyBitsMappings(type, payloadParameters);

            if (bitMappings != null)
            {
                foreach (var map in bitMappings)
                {
                    if (map.GetProperty(payloadParameters) == null)
                    {
                        if (!map.PreserveNotSetIgnoranceOnEncode)
                        {
                            continue;
                        }
                    }

                    if (maxIndex < map.StartByteIndex + 1)
                    {
                        maxIndex = map.StartByteIndex + 1;
                    }
                }
            }

            var byteMappings = controlItem.GetPropertyBytesMappings(type, payloadParameters);
            if (byteMappings != null) 
            {
                foreach (var map in byteMappings)
                {
                    if (!map.PropertyGetter(payloadParameters).HasValue)
                    {
                        if (!map.PreserveNotSetIgnoranceOnEncode)
                        {
                            continue;
                        }
                    }

                    int posIndex = map.ByteIndex;
                    int lastByteIndex = posIndex + map.BytesCount;

                    if (maxIndex < lastByteIndex)
                    {
                        maxIndex = lastByteIndex;
                    }
                }
            }
            
            return maxIndex;
        }

        private void BuildHeaderPart(Span<byte> messageSpan, ushort messageLength, byte kind, ushort controlItemCode)
        {
            var headerBytes = _controlItemHeader.GetHeaderBytes(messageLength, kind, controlItemCode);
            messageSpan[0] = headerBytes[0];
            messageSpan[1] = headerBytes[1];
            messageSpan[2] = headerBytes[2];
            messageSpan[3] = headerBytes[3];
        }

        private void BuildPayload<TPayload>(Span<byte> payloadSpan, RequestResponseType type, TPayload payloadParameters, IControlItem<TPayload> controlItem)
        {
            // Get PropertyBitsMappings relevant for current payload parameters
            var propertyToBitMappings = controlItem.GetPropertyBitsMappings(type, payloadParameters);
            if (propertyToBitMappings != null)
            {
                foreach (var map in propertyToBitMappings)
                {
                    object? propertyValue = map.GetProperty(payloadParameters);
                    byte propertyMappedByte = 0;

                    if (propertyValue == null)
                    {
                        if (!map.PreserveNotSetIgnoranceOnEncode)
                        {
                            // Skip the properties which were not set
                            continue;
                        }
                    }
                    else
                    {
                        propertyMappedByte = map.PossibleValuesToByteMap[propertyValue];
                    }
                    
                    _bitDecoder.EncodeByte(payloadSpan, map.StartByteIndex, propertyMappedByte);
                }
            }

            // Get PropertyBytesMappings relevant for current payload parameters
            var propertyToByteMappings = controlItem.GetPropertyBytesMappings(type, payloadParameters);
            if (propertyToByteMappings != null)
            {
                foreach (var map in propertyToByteMappings)
                {
                    ulong? propertyValue = map.PropertyGetter(payloadParameters);
                    ulong propertyMappedBytes = 0;

                    if (!propertyValue.HasValue)
                    {
                        if (!map.PreserveNotSetIgnoranceOnEncode)
                        {
                            // Skip the properties which were not set
                            continue;
                        }
                    }
                    else
                    {
                        propertyMappedBytes = propertyValue.Value;
                    }

                    _bitDecoder.EncodeULong(payloadSpan, map.ByteIndex, map.BytesCount, propertyMappedBytes);
                }
            }
        }

        public byte[] BuildSendMessage<TPayload>(RequestResponseType type, TPayload payloadParameters, IControlItem<TPayload> controlItem)
        {
            ushort messageLength = (ushort)(CalculatePayloadSize(type, payloadParameters, controlItem) + _controlItemHeader.HeaderAndCodeSize);
            Span<byte> messageSpan = stackalloc byte[messageLength];
            BuildHeaderPart(messageSpan, messageLength, (byte)type, controlItem.Code);
            BuildPayload(messageSpan.Slice(_controlItemHeader.HeaderAndCodeSize), type, payloadParameters, controlItem);
            return messageSpan.ToArray();
        }

        public TPayload? BuildReceiveMessage<TPayload>(RequestResponseType type, byte[] messageBytes, IControlItem<TPayload> controlItem) where TPayload : class, new()
        {
            // Check for NAK message
            if (_nakMessage.IsNAK(messageBytes))
            {
                return default;
            }

            int messageSize = _controlItemHeader.DecodeMessageLength(messageBytes[0], messageBytes[1]);
            int payloadSize = messageSize - _controlItemHeader.HeaderAndCodeSize;
            var messagePayloadParameters = new TPayload();
            var messageSpan = new Span<byte>(messageBytes, 0, messageSize);
            var payload = messageSpan.Slice(_controlItemHeader.HeaderAndCodeSize);

            // Get PropertyBitsMappings relevant for received byte array 
            var bitsMaps = controlItem.GetPropertyBitsMappings(type, contextMessageBytes: messageBytes);
            if (bitsMaps != null)
            {
                foreach (var map in bitsMaps)
                {
                    if (map.StartByteIndex >= payloadSize)
                    {
                        // Skip those mappings which don't fit incoming message's payload size
                        continue;
                    }
                    var possibleValueByte = _bitDecoder.DecodeByteByMask(/*rawMessage.Parameters*/payload, map.StartByteIndex, map.BitIndices);
                    if (map.PossibleValuesMap.TryGetValue(possibleValueByte, out var obj))
                    {
                        map.SetProperty(messagePayloadParameters, obj);
                    }
                }
            }

            // Get PropertyBytesMappings relevant for received byte array 
            var bytesMaps = controlItem.GetPropertyBytesMappings(type, contextPayloadBytes: messageBytes);
            if (bytesMaps != null)
            {
                foreach (var map in bytesMaps)
                {
                    if (map.ByteIndex + map.BytesCount > payloadSize)
                    {
                        // Skip those mappings which don't fit incoming message's payload size
                        continue;
                    }
                    ulong convertedValue = _bitDecoder.ConvertBytesToULong(/*rawMessage.Parameters*/payload, map.ByteIndex, map.BytesCount);
                    map.PropertySetter(messagePayloadParameters, convertedValue);
                }
            }

            return messagePayloadParameters;
        }
    }
}
