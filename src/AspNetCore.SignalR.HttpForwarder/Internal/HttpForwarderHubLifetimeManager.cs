using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.SignalR.HttpForwarder.Internal.Recipients;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCore.SignalR.HttpForwarder.Internal
{
    internal class HttpForwarderHubLifetimeManager<THub> : HubLifetimeManager<THub>
        where THub : Hub
    {
        private readonly MessageSenderHubLifetimeManager<THub> _messageSender;
        private readonly IForwarder _forwarder;

        public HttpForwarderHubLifetimeManager(
            MessageSenderHubLifetimeManager<THub> messageSender,
            IForwarder forwarder)
        {
            _messageSender = messageSender;
            _forwarder = forwarder;
        }

        public override Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            return _messageSender.AddToGroupAsync(connectionId, groupName, cancellationToken);
        }

        public override Task OnConnectedAsync(HubConnectionContext connection)
        {
            return _messageSender.OnConnectedAsync(connection);
        }

        public override Task OnDisconnectedAsync(HubConnectionContext connection)
        {
            return _messageSender.OnDisconnectedAsync(connection);
        }

        public override Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            return _messageSender.RemoveFromGroupAsync(connectionId, groupName, cancellationToken);
        }

        public override async Task SendAllAsync(string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            await _messageSender.SendAllAsync(methodName, args, cancellationToken);
            var message = CreateMessage(methodName, args, new AllMessageRecipient());
            await _forwarder.PublishAsync(message);
        }

        public override async Task SendAllExceptAsync(string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            await _messageSender.SendAllExceptAsync(methodName, args, excludedConnectionIds, cancellationToken);
            var message = CreateMessage(methodName, args, new AllMessageRecipient { ExcludedConnectionIds = excludedConnectionIds });
            await _forwarder.PublishAsync(message);
        }

        public override async Task SendConnectionAsync(string connectionId, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            await _messageSender.SendConnectionAsync(connectionId, methodName, args, cancellationToken);
            var message = CreateMessage(methodName, args, new ClientMessageRecipient { ConnectionId = connectionId });
            await _forwarder.PublishAsync(message);
        }

        public override async Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            await _messageSender.SendConnectionsAsync(connectionIds, methodName, args, cancellationToken);
            var message = CreateMessage(methodName, args, new ClientsMessageRecipient { ConnectionIds = connectionIds });
            await _forwarder.PublishAsync(message);
        }

        public override async Task SendGroupAsync(string groupName, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            await _messageSender.SendGroupAsync(groupName, methodName, args, cancellationToken);
            var message = CreateMessage(methodName, args, new GroupMessageRecipient { GroupName = groupName });
            await _forwarder.PublishAsync(message);
        }

        public override async Task SendGroupExceptAsync(string groupName, string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            await _messageSender.SendGroupExceptAsync(groupName, methodName, args, excludedConnectionIds, cancellationToken);
            var message = CreateMessage(methodName, args, new GroupMessageRecipient { GroupName = groupName, ExcludedConnectionIds = excludedConnectionIds });
            await _forwarder.PublishAsync(message);
        }

        public override async Task SendGroupsAsync(IReadOnlyList<string> groupNames, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            await _messageSender.SendGroupsAsync(groupNames, methodName, args, cancellationToken);
            var message = CreateMessage(methodName, args, new GroupsMessageRecipient { GroupNames = groupNames });
            await _forwarder.PublishAsync(message);
        }

        public override async Task SendUserAsync(string userId, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            await _messageSender.SendUserAsync(userId, methodName, args, cancellationToken);
            var message = CreateMessage(methodName, args, new UserMessageRecipient { UserId = userId });
            await _forwarder.PublishAsync(message);
        }

        public override async Task SendUsersAsync(IReadOnlyList<string> userIds, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            await _messageSender.SendUsersAsync(userIds, methodName, args, cancellationToken);
            var message = CreateMessage(methodName, args, new UsersMessageRecipient { UserIds = userIds });
            await _forwarder.PublishAsync(message);
        }

        private static SignalRMessage CreateMessage(string methodName, object[] args, params SignalRMessageRecipient[] recipients)
        {
            return new SignalRMessage
            {
                HubTypeName = typeof(THub).AssemblyQualifiedName,
                Method = methodName,
                Args = args,
                Recipients = recipients
            };
        }
    }
}
