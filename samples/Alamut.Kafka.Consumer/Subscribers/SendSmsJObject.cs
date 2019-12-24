using System;
using System.Threading;
using System.Threading.Tasks;
using Alamut.Kafka.Contracts;
using Alamut.Kafka.Models;
using Newtonsoft.Json.Linq;

namespace Alamut.Kafka.Consumer.Subscribers
{
    public class SendSmsJObject : IJObjectMessageHandler
    {
        public Task Handle(JObject message, CancellationToken token)
        {
            var foo = message.ToObject<Foo>();

            Console.WriteLine($"Received message { foo.Bar }");

            return Task.CompletedTask;
        }
    }
}