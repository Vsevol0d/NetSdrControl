using NetSdrControl.DataItems;
using NetSdrControl.Protocol;

namespace NetSdrControl.ControlItems
{
    public class ReceiverStateItem : IControlItem<ReceiverStateMessagePayload>
    {
        private INetSdrTcpClient _client;
        private IMessageBuilder _messageBuilder;
        private IBitDecoder _bitsDecoder;
        private DataExchanger _currentDataItemsExchanger;

        public ushort Code => 0x0018;

        public ReceiverStateItem(INetSdrTcpClient client, IMessageBuilder messageBuilder, IBitDecoder bitsDecoder, DataExchanger dataExchanger)
        {
            _client = client;
            _bitsDecoder = bitsDecoder;
            _messageBuilder = messageBuilder;
            _currentDataItemsExchanger = dataExchanger;
        }

        public async Task<bool> StartIQTransfer(NetSdrDataMode dataMode, NetSdrCaptureMode mode, NetSdrCaptureWay way, byte fifoSamplesCount)
        {
            _currentDataItemsExchanger.StartWriting();

            var message = new ReceiverStateMessagePayload(true, fifoSamplesCount, dataMode, mode, way);

            //if (!(await GetIQTransferState(message)).Value)
            //{
                var rawMessageBytes = _messageBuilder.BuildSendMessage(RequestResponseKind.Set, message, this);
                var responseMessageBytes = await _client.Send(rawMessageBytes);
                var responsePayload = _messageBuilder.BuildReceiveMessage(responseMessageBytes, this);
                if (!(bool)responsePayload?.IsRun.HasValue || !(bool)responsePayload?.IsRun.Value)
                {
                    throw new Exception("I/Q transferring cannot be started because of Target issue");
                }
            //}
            //else
            //{
            //    Console.WriteLine("I/Q transferring is already started");
            //}
            return true;
        }

        public async Task StopIQTransfer()
        {
            var message = new ReceiverStateMessagePayload();
            message.IsRun = false;

            //if ((await GetIQTransferState(message)).Value)
            //{
                var rawMessageBytes = _messageBuilder.BuildSendMessage(RequestResponseKind.Set, message, this);
                var responseMessageBytes = await _client.Send(rawMessageBytes);
                var responsePayload = _messageBuilder.BuildReceiveMessage(responseMessageBytes, this);
                if (!(bool)responsePayload?.IsRun.HasValue || !(bool)responsePayload?.IsRun.Value)
                {
                    throw new Exception("I/Q transferring cannot be started because of Target issue");
                }
            //}
            //else
            //{
            //    Console.WriteLine("I/Q transferring is already stopped");
            //}
            _currentDataItemsExchanger.StopWriting();
        }
        public async Task<bool?> GetIQTransferState(ReceiverStateMessagePayload message)
        {
            var rawMessageBytes = _messageBuilder.BuildSendMessage(RequestResponseKind.Get, message, this);
            var responseMessageBytes = await _client.Send(rawMessageBytes);
            var responsePayload = _messageBuilder.BuildReceiveMessage(responseMessageBytes, this);
            return responsePayload?.IsRun;
        }

        public List<IPropertyBitsMapping> GetPropertyBitsMappings(ReceiverStateMessagePayload contextParameters, byte[] contextMessageBytes = null)
        {
            var sampleTypeMapping = new PropertyBitsMapping<ReceiverStateMessagePayload, NetSdrDataMode?>(_bitsDecoder, 0, [7],
                (payload, dataMode) => { payload.DataMode = dataMode; }, (payload) => { return payload.DataMode; },
                [([0], NetSdrDataMode.RealADSample), ([1], NetSdrDataMode.ComplexIQBaseBand)]);

            var runStopMapping = new PropertyBitsMapping<ReceiverStateMessagePayload, bool?>(_bitsDecoder, 1, [0, 1],
                (payload, isRun) => { payload.IsRun = isRun; }, (payload) => { return payload.IsRun; },
                [([1, 0], false), ([0, 1], true)]);

            var captureWayMapping = new PropertyBitsMapping<ReceiverStateMessagePayload, NetSdrCaptureWay?>(_bitsDecoder, 2, [0, 1],
                (payload, captureWay) => { payload.Way = captureWay; }, (payload) => { return payload.Way; },
                [([0, 0], NetSdrCaptureWay.Contiguous), ([1, 0], NetSdrCaptureWay.FIFO), ([1, 1], NetSdrCaptureWay.Hardware)]);

            var captureModeMapping = new PropertyBitsMapping<ReceiverStateMessagePayload, NetSdrCaptureMode?>(_bitsDecoder, 2, [7],
                (payload, captureMode) => { payload.Mode = captureMode; }, (payload) => { return payload.Mode; },
                [([0], NetSdrCaptureMode.x_16), ([1], NetSdrCaptureMode.x_24)]);

            return new List<IPropertyBitsMapping>() { sampleTypeMapping, runStopMapping, captureWayMapping, captureModeMapping };
        }

        public List<PropertyBytesMapping> GetPropertyBytesMappings(ReceiverStateMessagePayload contextParameters, byte[] contextMessageBytes = null)
        {
            return new List<PropertyBytesMapping> { new PropertyBytesMapping(3, 1,
                (payload, samplesCount) => { ((ReceiverStateMessagePayload)payload).FifoSamplesCount = (byte)samplesCount; },
                (payload) => { return ((ReceiverStateMessagePayload)payload).FifoSamplesCount; }) };
        }

        public byte[] GetSamples()
        {
            return _currentDataItemsExchanger?.GetSamples();
        }
    }

    public enum NetSdrDataMode
    {
        RealADSample, ComplexIQBaseBand
    }

    public enum NetSdrCaptureMode : byte
    {
        x_16, x_24
    }
    public enum NetSdrCaptureWay : byte
    {
        Contiguous, FIFO, Hardware
    }

    public class ReceiverStateMessagePayload
    {
        public ReceiverStateMessagePayload(bool isRun, byte fifoSamplesCount, NetSdrDataMode dataMode, NetSdrCaptureMode mode, NetSdrCaptureWay way)
        {
            FifoSamplesCount = fifoSamplesCount;
            DataMode = dataMode;
            IsRun = isRun;
            Mode = mode;
            Way = way;
        }
        public ReceiverStateMessagePayload(bool isRun)
        {
            IsRun = isRun;
        }
        public ReceiverStateMessagePayload()
        {
        }

        public bool? IsRun { get; set; }
        public NetSdrDataMode? DataMode { get; set; }
        public NetSdrCaptureMode? Mode { get; set; }
        public NetSdrCaptureWay? Way { get; set; }
        public byte? FifoSamplesCount { get; set; }
    }
}
