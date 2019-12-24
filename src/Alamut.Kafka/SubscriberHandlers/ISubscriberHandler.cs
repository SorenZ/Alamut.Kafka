using System.Threading;
using System.Threading.Tasks;

using Confluent.Kafka;

namespace Alamut.Kafka.SubscriberHandlers
{
    /// <summary>
    /// provides a contract on how to handle a Kafka Message
    /// </summary>
    public interface ISubscriberHandler
    {
        /// <summary>
        /// called whenever a Kafka Message has arrived
        /// </summary>
        /// <param name="result"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task HandleMessage(ConsumeResult<Ignore, string> result, CancellationToken token);
    }
}