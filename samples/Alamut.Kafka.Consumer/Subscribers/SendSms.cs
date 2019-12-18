using System;
using System.Threading;
using System.Threading.Tasks;
using Alamut.Kafka.Contracts;

namespace Alamut.Kafka.Consumer.Subscribers
{
    public class SendSms : IStringSubscriber
    {
        public Task Handle(string message, CancellationToken token)
        {
            Console.WriteLine($"Received message {message }");

            return Task.CompletedTask;
        }
    }
}