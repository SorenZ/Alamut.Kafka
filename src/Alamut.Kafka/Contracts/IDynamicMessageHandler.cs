using System.Threading;
using System.Threading.Tasks;

namespace Alamut.Kafka.Contracts
{
    public interface IDynamicMessageHandler 
    {
        Task Handle(dynamic message, CancellationToken token);
    }
}