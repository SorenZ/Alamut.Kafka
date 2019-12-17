using System;
using System.Threading;
using System.Threading.Tasks;
using Alamut.Kafka.Contracts;

namespace Alamut.Kafka.Consumer.Subscribers
{
    public class SendSmsDynamic : IDynamicSubscriber
    {
        public Task Handle(dynamic message, CancellationToken token)
        {
            Console.WriteLine($"Received message { message.Foo }");

            return Task.CompletedTask;
        }
    }
}