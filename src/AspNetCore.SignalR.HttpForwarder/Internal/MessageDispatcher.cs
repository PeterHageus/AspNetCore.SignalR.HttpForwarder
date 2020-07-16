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

        public MessageDispatcher(
            IMessageSenderProvider messageSenderProvider,
            ILogger<MessageDispatcher> logger)
        {
            _messageSenderProvider = messageSenderProvider;
            _logger = logger;
        }

        public async Task OnMessageReceived(SignalRMessage message, CancellationToken cancellationToken)
        {
            var messageSender = _messageSenderProvider.GetMessageSenderForHub(message.HubTypeName);
            if (messageSender == null)
            {
                _logger.LogWarning("Can't find message sender for hub '{HubTypeName}'", message.HubTypeName);
                return;
            }

            foreach(var recipient in message.Recipients)
            {
                await recipient.SendCoreAsync(messageSender, message.Method, message.Args, cancellationToken);
            }
        }
    }
}
