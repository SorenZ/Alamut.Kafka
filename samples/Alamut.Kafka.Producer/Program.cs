using System;
using System.Threading.Tasks;

using Alamut.Abstractions.Messaging;
using Alamut.Kafka.Models;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alamut.Kafka.Producer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();


            Console.WriteLine("start Kafka producer");

            IPublisher publisher = new KafkaProducer(
                new ProducerConfig { BootstrapServers = "10.104.51.12:9092,10.104.51.13:9092,10.104.51.14:9092" },
                serviceProvider.GetService<ILogger<KafkaProducer>>());
            
            while (true)
            {
                var message = Console.ReadLine();

                // publish (inherited) Message
                // var typedMessage = new FooMessage
                // {
                //     // Id = IdGenerator.GetNewId(), // generate automatically in publisher
                //     EventName = "Alamut.Foo.Create",
                //     Bar = message,
                //     AcknowledgeRequested = true,
                //     AcknowledgeTopic = "mobin-soft"
                // };
                
                // public (generic) message
                var typedMessage = new Foo
                {
                    Bar = message
                };

                await publisher.Publish("mobin-net", MessageFactory.Build(typedMessage));
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => {
                configure.AddConsole();
                configure.SetMinimumLevel(LogLevel.Trace);
            });
                    
        }
    }
}
