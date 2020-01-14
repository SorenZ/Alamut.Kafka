using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Alamut.Abstractions.Messaging;
using Alamut.Abstractions.Messaging.Handlers;
using Alamut.Kafka.DependencyInjection;
using Alamut.Kafka.SubscriberHandlers;

using Confluent.Kafka;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alamut.Kafka
{
    public static class KafkaSubscriberExtensions
    {
        /// <summary>
        /// adds a new Kafka subscriber as HostedService (long running back-ground service)
        /// </summary>
        /// <param name="services"></param>
        /// <param name="topics">topics to subscribes, if it's not provided subscribes to all topics in KafkaConfig</param>
        /// <returns></returns>
        public static IServiceCollection AddNewHostedSubscriber(this IServiceCollection services, params string[] topics)
            => AddNewHostedSubscriber(services, new List<string>(topics));

        /// <summary>
        /// adds a new Kafka subscriber as HostedService (long running back-ground service)
        /// </summary>
        /// <param name="services"></param>
        /// <param name="topics">topics to subscribes, if it's not provided subscribes to all topics in KafkaConfig</param>
        /// <returns></returns>
        public static IServiceCollection AddNewHostedSubscriber(this IServiceCollection services, IEnumerable<string> topics)
        {
            return services.AddTransient<IHostedService>(provider =>
            {
                var config = provider.GetRequiredService<ConsumerConfig>();

                if (topics == null)
                { throw new ArgumentNullException(nameof(topics)); }

                return new KafkaSubscriber(
                    provider.GetRequiredService<ILoggerFactory>(),
                    config,
                    provider.GetRequiredService<ISubscriberHandler>(),
                    topics);
            });
        }

        /// <summary>
        /// - adds a new Kafka subscriber as HostedService (long running back-ground service)  
        /// - register topics previously discovered by SubscriberBinding
        /// </summary>
        /// <param name="services"></param>
        /// <returns>
        /// registered topics
        /// </returns>
        /// <remarks>
        /// - register topics previously discovered by SubscriberBinding
        /// - it should be registered once
        /// </remarks>
         public static IList<string> AddHostedSubscriber(this IServiceCollection services)
        {
            IList<string> topics = null;

            services.AddTransient<IHostedService>(provider =>
            {
                var config = provider.GetRequiredService<ConsumerConfig>();
                
                var subscriberBinding = provider.GetRequiredService<SubscriberBinding>();
                topics = subscriberBinding.RegisteredTopics;

                if (topics == null)
                { throw new ArgumentNullException(nameof(topics)); }

                return new KafkaSubscriber(
                    provider.GetRequiredService<ILoggerFactory>(),
                    config,
                    provider.GetRequiredService<ISubscriberHandler>(),
                    topics);
            });

            return topics;
        }


        /// <summary>
        /// registers all MessageHandler in Assembly specified by type parameter
        /// </summary>
        /// <typeparam name="TMessageHandler">type of </typeparam>
        /// <returns>
        /// registered topics
        /// </returns>
        public static IList<string> RegisterMessageHandlers<TMessageHandler>(this IServiceCollection services)
            where TMessageHandler : IMessageHandler
            => RegisterMessageHandlers(services, typeof(TMessageHandler).Assembly);

        /// <summary>
        /// register all MessageHandler in specified assemblies
        /// map MessageHandler with their own Topic(s) 
        /// register GenericSubscriberHandler
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <returns>
        /// registered topics
        /// </returns>
        public static IList<string> RegisterMessageHandlers(this IServiceCollection services, params Assembly[] assemblies)
        {
            var registeredTopics = new List<string>();
            var subscriberBinding = new SubscriberBinding();

            var types = KafkaHelper.GetClassesImplementingAnInterface(assemblies, typeof(IMessageHandler<>));

            foreach (var messageHandlerType in types)
            {
                // https://docs.microsoft.com/en-us/dotnet/api/system.type.getinterface
                var messageType = messageHandlerType.GetInterface(typeof(IMessageHandler<>).Name).GetGenericArguments()[0];

                var topics = messageHandlerType.GetCustomAttribute<TopicsAttribute>()?.Topics
                    ?? throw new Exception($"{nameof(TopicsAttribute)} does not defined for MessageHandler : {messageHandlerType.Name}");

                subscriberBinding.RegisterTopicHandler(messageHandlerType, messageType, topics);

                services.AddScoped(messageHandlerType);

                registeredTopics.AddRange(topics);
            }

            services.AddSingleton(subscriberBinding);
            services.AddSingleton<ISubscriberHandler, GenericSubscriberHandler>();

            return registeredTopics;
        }
        
    }


}