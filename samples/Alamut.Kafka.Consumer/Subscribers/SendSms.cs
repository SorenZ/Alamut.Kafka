using System;
using System.Threading;
using System.Threading.Tasks;
using Alamut.Kafka.Contracts;
using Alamut.Kafka.Models;

namespace Alamut.Kafka.Consumer.Subscribers
{
    public class SendSms : ISubscriber
    {
        public Task Handle(string message, CancellationToken token)
        {
            Console.WriteLine($"Received message {message }");

            return Task.CompletedTask;
        }
    }

    public class SendSmsDynamic : IDynamicSubscriber
    {
        public Task Handle(dynamic message, CancellationToken token)
        {
            Console.WriteLine($"Received message { message.Foo }");

            return Task.CompletedTask;
        }
    }
}