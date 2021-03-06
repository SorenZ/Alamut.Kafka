using System;
using System.Threading;
using System.Threading.Tasks;

using Alamut.Abstractions.Messaging.Handlers;

namespace Alamut.Kafka.Consumer.Subscribers
{
    public class SendSms : IStringMessageHandler
    {
        public Task Handle(string message, CancellationToken token)
        {
            Console.WriteLine($"Received message {message }");

            return Task.CompletedTask;
        }
    }
}