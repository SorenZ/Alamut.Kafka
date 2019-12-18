using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Alamut.Kafka.SubscriberHandlers
{
    public static class KafkaSubscriberRegistrationExtensions
    {
        public static IDictionary<string, KeyValuePair<Type,Type>> TopicHandlers { get; private set; }
        static KafkaSubscriberRegistrationExtensions()
        {
            TopicHandlers = new Dictionary<string, KeyValuePair<Type,Type>>();
        }

        public static IServiceCollection RegisterSubscriberHandler<TSubscriber, TSubscriberDataStructure>(
            this IServiceCollection services, string topic) 
            where TSubscriber : class
        {
            if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }
            
            TopicHandlers[topic] = new KeyValuePair<Type, Type>(typeof(TSubscriber),typeof(TSubscriberDataStructure));
            
            services.AddScoped<TSubscriber>();
            
            return services;
        }
    }


}