﻿using NetSdrControl.Protocol;

namespace NetSdrControl.ControlItems
{
    public class ReceiverFrequencyControlItem : IControlItem<ReceiverFrequencyMessagePayload>
    {
        private INetSdrTcpClient _client;
        private MessageBuilder _messageBuilder;
        private IBitDecoder _bitsDecoder;

        public ushort Code => 0x0020;

        public ReceiverFrequencyControlItem(INetSdrTcpClient client, MessageBuilder messageBuilder, IBitDecoder bitsDecoder)
        {
            _client = client;
            _bitsDecoder = bitsDecoder;
            _messageBuilder = messageBuilder;
        }

        public List<IPropertyBitsMapping> GetPropertyBitsMappings(ReceiverFrequencyMessagePayload contextParameters, byte[] contextMessageBytes = null)
        {
            var channelMapping = new PropertyBitsMapping<ReceiverFrequencyMessagePayload, FrequencyChannel?>(_bitsDecoder, 0, [0, 1],
                (payload, channel) => { payload.FrequencyChannel = channel; }, (payload) => { return payload.FrequencyChannel; },
                [([0, 0], FrequencyChannel.Channel_1), ([0, 1], FrequencyChannel.Channel_2)]);
            return new List<IPropertyBitsMapping>() { channelMapping };
        }

        public List<PropertyBytesMapping> GetPropertyBytesMappings(ReceiverFrequencyMessagePayload contextParameters, byte[] contextMessageBytes = null)
        {
            return new List<PropertyBytesMapping> { new PropertyBytesMapping(1, 5, 
                (payload, centerFrequency) => { ((ReceiverFrequencyMessagePayload)payload).CenterFrequencyValue = centerFrequency; }, 
                (payload) => { return ((ReceiverFrequencyMessagePayload)payload).CenterFrequencyValue; }) };
        }

        public async Task<ulong?> GetFrequency(FrequencyChannel frequencyChannel)
        {
            var message = new ReceiverFrequencyMessagePayload(frequencyChannel);
            var rawMessageBytes = _messageBuilder.BuildSendMessage(RequestResponseKind.Get, message, this);
            var responseMessageBytes = await _client.Send(rawMessageBytes);
            var responsePayload = _messageBuilder.BuildReceiveMessage(responseMessageBytes, this);
            return responsePayload?.CenterFrequencyValue;
        }

        public async Task<ulong?> ChangeFrequency(FrequencyChannel frequencyChannel, ulong centerFrequencyValue)
        {
            var message = new ReceiverFrequencyMessagePayload(frequencyChannel, centerFrequencyValue);
            var rawMessageBytes = _messageBuilder.BuildSendMessage(RequestResponseKind.Set, message, this);
            var responseMessageBytes = await _client.Send(rawMessageBytes);
            var responsePayload = _messageBuilder.BuildReceiveMessage(responseMessageBytes, this);
            return responsePayload?.CenterFrequencyValue;
        }
    }

    public enum FrequencyChannel
    {
        Channel_1, Channel_2, All
    }

    public class ReceiverFrequencyMessagePayload
    {
        public FrequencyChannel? FrequencyChannel { get; set; }
        public ulong? CenterFrequencyValue { get; set; }

        public ReceiverFrequencyMessagePayload()
        {
        }

        /// <summary>
        /// Constructor for setting frequency
        /// </summary>
        /// <param name="frequencyChannel"></param>
        /// <param name="frequencyValue"></param>
        public ReceiverFrequencyMessagePayload(FrequencyChannel frequencyChannel, ulong frequencyValue)
        {
            FrequencyChannel = frequencyChannel;
            CenterFrequencyValue = frequencyValue;
        }

        /// <summary>
        /// Constructor for getting frequency
        /// </summary>
        /// <param name="frequencyChannel"></param>
        public ReceiverFrequencyMessagePayload(FrequencyChannel frequencyChannel)
        {
            FrequencyChannel = frequencyChannel;
        }
    }
}
