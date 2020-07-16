using System;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.SignalR.HttpForwarder.Internal;
using AspNetCore.SignalR.HttpForwarder.Internal.Recipients;
using FakeItEasy;
using Xunit;

namespace AspNetCore.SignalR.HttpForwarder.UnitTests.Recipients
{
    public class AllMessageRecipientTests
    {
        [Fact]
        public async Task SendCoreAsync_Sends_Message_To_All_Recipients()
        {
            await RecipientTestHelper.AssertSendAsync(
                new AllMessageRecipient(),
                (sender, methodName, args) =>
                    () => sender.SendAllExceptAsync(methodName, args, Array.Empty<string>(), CancellationToken.None));
        }

        [Fact]
        public async Task SendCoreAsync_Sends_Message_To_All_Recipients_Except_Excluded_Connections()
        {
            var excludedConnectionIds = new[] { "abcd", "efgh" };

            await RecipientTestHelper.AssertSendAsync(
                new AllMessageRecipient { ExcludedConnectionIds = excludedConnectionIds },
                (sender, methodName, args) =>
                    () => sender.SendAllExceptAsync(methodName, args, excludedConnectionIds, CancellationToken.None));
        }
    }
}
