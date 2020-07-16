using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace AspNetCore.SignalR.HttpForwarder.Internal
{
    internal class MessageForwarderBootstrapper : IHostedService
    {
        private readonly MessageForwarder _messageForwarder;
        public MessageForwarderBootstrapper(MessageForwarder messageForwarder) => _messageForwarder = messageForwarder;
        public Task StartAsync(CancellationToken cancellationToken) => _messageForwarder.StartAsync(cancellationToken);
        public Task StopAsync(CancellationToken cancellationToken) => _messageForwarder.StopAsync(cancellationToken);
    }
}
