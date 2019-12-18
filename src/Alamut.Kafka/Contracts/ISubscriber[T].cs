using System.Threading;
using System.Threading.Tasks;

namespace Alamut.Kafka.Contracts
{
    public interface ISubscriber<T>
    {
        Task Handle(T message, CancellationToken token);
    }
}