using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AspNetCore.SignalR.HttpForwarder.Internal
{
    internal class MessageDispatcher
    {
        private readonly IMessageSenderProvider _messageSenderProvider;
        private readonly ILogger<MessageDispatcher> _logger;
        private readonly IObserver<MessageHook> _hook;

        public MessageDispatcher(
            IMessageSenderProvider messageSenderProvider,
            ILogger<MessageDispatcher> logger, 
            IObserver<MessageHook> hook)
        {
            _messageSenderProvider = messageSenderProvider;
            _logger = logger;
            _hook = hook;
        }

        public async Task OnMessageReceived(SignalRMessage message, CancellationToken cancellationToken)
        {
            var messageSender = _messageSenderProvider.GetMessageSenderForHub(message.HubTypeName);
            if (messageSender == null)
            {
                _logger.LogWarning("Can't find message sender for hub '{HubTypeName}'", message.HubTypeName);
                return;
            }

            _hook.OnNext(new MessageHook(message.HubTypeName, message.Method, message.Args));

            foreach(var recipient in message.Recipients)
            {
                _logger.LogDebug("Dispatched {MethodName} to {Hub}", message.Method, message.HubTypeName);
                await recipient.SendCoreAsync(messageSender, message.Method, message.Args, cancellationToken);
            }
        }
    }
}
