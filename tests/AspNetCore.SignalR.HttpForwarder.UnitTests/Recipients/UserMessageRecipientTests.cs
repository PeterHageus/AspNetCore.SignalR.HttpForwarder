using System.Threading;
using System.Threading.Tasks;
using AspNetCore.SignalR.HttpForwarder.Internal.Recipients;
using Xunit;

namespace AspNetCore.SignalR.HttpForwarder.UnitTests.Recipients
{
    public class UserMessageRecipientTests
    {
        [Fact]
        public async Task SendCoreAsync_Sends_Message_To_Specified_User()
        {
            string userId = "abcd";

            await RecipientTestHelper.AssertSendAsync(
                new UserMessageRecipient { UserId = userId },
                (sender, methodName, args) =>
                    () => sender.SendUserAsync(userId, methodName, args, CancellationToken.None));
        }
    }
}
