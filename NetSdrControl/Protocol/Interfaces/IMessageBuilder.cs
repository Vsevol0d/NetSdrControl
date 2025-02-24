using NetSdrControl.ControlItems;

namespace NetSdrControl.Protocol.Interfaces
{
    public interface IMessageBuilder
    {
        byte[] BuildSendMessage<TPayload>(RequestResponseType kind, TPayload payloadParameters, IControlItem<TPayload> controlItem);
        TPayload? BuildReceiveMessage<TPayload>(RequestResponseType kind, byte[] messageBytes, IControlItem<TPayload> controlItem) where TPayload : class, new();
    }
}
