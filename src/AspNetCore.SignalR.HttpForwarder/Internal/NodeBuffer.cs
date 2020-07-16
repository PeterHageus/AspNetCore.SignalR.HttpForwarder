using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AspNetCore.SignalR.HttpForwarder.Internal
{
    internal class NodeBuffer
    {
        public Node Node { get; }

        private readonly Channel<SignalRMessage> _buffer = Channel.CreateBounded<SignalRMessage>(new BoundedChannelOptions(50000)
        {
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = true
        });

        public NodeBuffer(Node node)
        {
            Node = node;
        }

        public ValueTask Add(SignalRMessage message) => _buffer.Writer.WriteAsync(message);

        public IAsyncEnumerable<SignalRMessage> GetMessages(CancellationToken cancellationToken) => _buffer.Reader.ReadAllAsync(cancellationToken);
    }
}
