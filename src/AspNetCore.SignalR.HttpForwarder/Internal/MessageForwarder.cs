using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AspNetCore.SignalR.HttpForwarder.Internal
{
    internal class MessageForwarder : IForwarder
    {
        private readonly ConcurrentBag<NodeBuffer> _buffers = new ConcurrentBag<NodeBuffer>();
        private readonly ILogger<MessageForwarder> _log;
        private readonly SignalRHttpForwarderOptions _options;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly List<Task> _running = new List<Task>();
        private readonly IOtherNodesProvider _other;

        public MessageForwarder(ILogger<MessageForwarder> log, SignalRHttpForwarderOptions options, IOtherNodesProvider other)
        {
            _log = log;
            _options = options;
            _other = other;
        }

        private async Task PublishAsync(Node node, SignalRMessage message)
        {
            var body = JsonConvert.SerializeObject(message, _options.SerializerSettings);
            var uri = new Uri(node.Uri.ToString().TrimEnd('/') + SignalRHttpForwarderServiceCollectionExtensions.Endpoint);
            
            _log.LogDebug("Sending {@Payload} to {Uri}", message, node);

            try
            {
                await node
                    .Client()
                    .PostAsync(uri, new StringContent(body));
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error sending message to {Uri}", node);
            }
        }

        public Task StartAsync(CancellationToken _)
        {
            _other
                .Nodes()
                .Subscribe(StartNode);

            return Task.CompletedTask;
        }

        private void StartNode(Node node)
        {
            _log.LogInformation("Starting node {Uri}", node.Uri);

            var nodeBuffer = new NodeBuffer(node);
            _buffers.Add(nodeBuffer);

            _running.Add(Task.Factory.StartNew(async () =>
            {
                await foreach (var message in nodeBuffer.GetMessages(_cancellationTokenSource.Token))
                    await PublishAsync(nodeBuffer.Node, message);
            }, TaskCreationOptions.LongRunning));
        }

        public async Task StopAsync(CancellationToken _)
        {
            _cancellationTokenSource.Cancel();
            await Task.WhenAll(_running);
        }

        public async Task PublishAsync(SignalRMessage message)
        {
            foreach (var other in _buffers)
                await other.Add(message);
        }
    }
}
