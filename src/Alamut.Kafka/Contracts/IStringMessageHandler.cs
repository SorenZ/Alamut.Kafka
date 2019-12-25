using System.Threading;
using System.Threading.Tasks;

namespace Alamut.Kafka.Contracts
{
    public interface IStringMessageHandler
    {
        Task Handle(string message, CancellationToken token);
    }
}