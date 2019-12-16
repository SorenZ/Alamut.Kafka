using System.Threading;
using System.Threading.Tasks;

namespace Alamut.Kafka.Contracts
{
    public interface IGenericSubscriber<T>
    {
        Task Handle(T message, CancellationToken token);
    }
}