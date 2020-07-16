using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCore.SignalR.HttpForwarder.TestApp.Hubs
{
    public class ChatroomHub : Hub<IChatroomClient>
    {
        public async Task SendMessageAsync(string message)
        {
            await Clients.Others.ReceiveMessage(message);
        }
    }

    public interface IChatroomClient
    {
        Task ReceiveMessage(string message);
    }
}
