using BlazorServerDemo.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorServerDemo.Services
{
    public interface IPublishService
    {
        Task<Message> SendMessage(string message, CancellationToken token);
    }
}
