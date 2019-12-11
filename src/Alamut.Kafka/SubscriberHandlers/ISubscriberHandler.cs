using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Alamut.Kafka.SubscriberHandlers
{
    public interface ISubscriberHandler
    {
        Task HandleMessage(ConsumeResult<Ignore, string> result, CancellationToken token);
    }
}