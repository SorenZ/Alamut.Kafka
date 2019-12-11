using System.Threading;
using System.Threading.Tasks;

namespace Alamut.Kafka.Contracts
{
    public interface IDynamicSubscriber 
    {
        Task Handle(dynamic message, CancellationToken token);
    }
}