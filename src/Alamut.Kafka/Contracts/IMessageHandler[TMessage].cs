using System.Threading;
using System.Threading.Tasks;

namespace Alamut.Kafka.Contracts
{
    public interface IMessageHandler<TMessage>
    {
        Task Handle(TMessage message, CancellationToken token);
    }
}