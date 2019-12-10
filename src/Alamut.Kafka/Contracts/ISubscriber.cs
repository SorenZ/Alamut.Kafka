using System.Threading;
using System.Threading.Tasks;
using Alamut.Kafka.Models;

namespace Alamut.Kafka.Contracts
{
    public interface ISubscriber
    {
        Task Handle(string message, CancellationToken token);
    }
}