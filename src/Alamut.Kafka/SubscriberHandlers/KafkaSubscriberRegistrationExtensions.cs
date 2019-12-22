using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Alamut.Kafka.SubscriberHandlers
{
    public static class KafkaSubscriberRegistrationExtensions
    {
        public static IServiceCollection RegisterSubscriberHandler<TSubscriber, TSubscriberDataStructure>(
            this IServiceCollection services, string topic, SubscriberBinding binding) 
            where TSubscriber : class
        {
            if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }
            
            binding.GenericTopicHandlers[topic] = new KeyValuePair<Type, Type>(typeof(TSubscriber),typeof(TSubscriberDataStructure));
            
            services.AddScoped<TSubscriber>();
            
            return services;
        }
    }


}