using System.Threading.Tasks;

namespace AspNetCore.SignalR.HttpForwarder.Internal
{
    internal interface IForwarder
    {
        Task PublishAsync(SignalRMessage message);
    }
}
