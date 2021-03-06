using System;
using System.Threading;
using System.Threading.Tasks;

using Alamut.Abstractions.Messaging.Handlers;

namespace Alamut.Kafka.Consumer.Subscribers
{
    public class SendSmsDynamic : IDynamicMessageHandler
    {
        public Task Handle(dynamic message, CancellationToken token)
        {
            Console.WriteLine($"Received message { message.Foo }");

            return Task.CompletedTask;
        }
    }
}