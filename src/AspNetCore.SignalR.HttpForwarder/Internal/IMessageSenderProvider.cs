namespace AspNetCore.SignalR.HttpForwarder.Internal
{
    internal interface IMessageSenderProvider
    {
        IMessageSender GetMessageSenderForHub(string hubTypeName);
    }
}
