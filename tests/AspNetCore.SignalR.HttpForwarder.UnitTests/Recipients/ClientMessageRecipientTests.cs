using System.Threading;
using System.Threading.Tasks;
using AspNetCore.SignalR.HttpForwarder.Internal.Recipients;
using Xunit;

namespace AspNetCore.SignalR.HttpForwarder.UnitTests.Recipients
{
    public class ClientMessageRecipientTests
    {
        [Fact]
        public async Task SendCoreAsync_Sends_Message_To_Specified_Client()
        {
            string connectionId = "abcd";

            await RecipientTestHelper.AssertSendAsync(
                new ClientMessageRecipient { ConnectionId = connectionId },
                (sender, methodName, args) =>
                    () => sender.SendConnectionAsync(connectionId, methodName, args, CancellationToken.None));
        }
    }
}
