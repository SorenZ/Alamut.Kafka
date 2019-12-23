using System;
using System.Threading.Tasks;
using Alamut.Helpers.DomainDriven;
using Alamut.Kafka.Contracts;
using Alamut.Kafka.Models;
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
                new Models.KafkaConfig { BootstrapServers = "10.104.51.12:9092,10.104.51.13:9092,10.104.51.14:9092" },
                serviceProvider.GetService<ILogger<KafkaProducer>>());
            
            while (true)
            {
                var message = Console.ReadLine();
                // var dynamicMessage = new Foo
                // {
                //     Bar = message
                // };
                var dynamicMessage = new FooMessage
                {
                    Id = IdGenerator.GetNewId(),
                    EventName = "Alamut.Foo.Create",
                    Bar = message
                };
                await publisher.Publish("mobin-soft", dynamicMessage);
                // await publisher.Publish("mobin-soft", "send sms to 0912");
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
