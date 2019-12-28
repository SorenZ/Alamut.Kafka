using System.Threading.Tasks;
using Alamut.Abstractions.Messaging;
using Alamut.Kafka.Models;

namespace Alamut.Kafka.Contracts
{
    public interface IPublisher
    {
        Task Publish(string topic, string message);
        Task Publish(string topic, object message);
        Task Publish(string topic, IMessage message);
    }
}