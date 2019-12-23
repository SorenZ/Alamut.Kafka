using System.Linq;
using Alamut.Kafka.Models;
using Alamut.Kafka.SubscriberHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alamut.Kafka
{
    public static class KafkaSubscriberExtensions
    {
        public static IServiceCollection AddNewHostedSubscriber(this IServiceCollection services, 
        string groupId = null, 
        params string[] topics)
        {
            return services.AddTransient<IHostedService>(provider => 
            {
                
                var config = provider.GetRequiredService<KafkaConfig>();
                
                if(!string.IsNullOrEmpty(groupId))
                { config.GroupId = groupId; }

                if(topics != null && topics.Any())
                { config.Topics = topics; }


                return new KafkaSubscriber(
                    provider.GetRequiredService<ILoggerFactory>(),
                    config,
                    provider.GetRequiredService<ISubscriberHandler>());
            });
        }
    }
}