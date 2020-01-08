using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Alamut.Abstractions.Messaging;
using Alamut.Abstractions.Messaging.Handlers;
using Alamut.Kafka.Models;
using Alamut.Kafka.SubscriberHandlers;

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
        /// <param name="groupId">group Id to subscribes with it, if it's not provided use groupId in KafkaConfig</param>
        /// <param name="topics">topics to subscribes, if it's not provided subscribes to all topics in KafkaConfig</param>
        /// <returns></returns>
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

                // TODO : check topics and groups for null and emptry


                return new KafkaSubscriber(
                    provider.GetRequiredService<ILoggerFactory>(),
                    config,
                    provider.GetRequiredService<ISubscriberHandler>());
            });
        }


        /// <summary>
        /// registers all MessageHandler in Assembly specified by type parameter
        /// </summary>
        /// <typeparam name="TMessageHandler">type of </typeparam>
        /// <returns></returns>
        public static IServiceCollection RegisterMessageHandlers<TMessageHandler>(this IServiceCollection services) 
            where TMessageHandler : IMessageHandler 
            => RegisterMessageHandlers(services, typeof(TMessageHandler).Assembly);

        /// <summary>
        /// register all MessageHandler in specified assemblies
        /// map MessageHandler with their own Topic(s) 
        /// register GenericSubscriberHandler
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterMessageHandlers(this IServiceCollection services, params Assembly[] assemblies)
        {
            var subscriberBinding = new SubscriberBinding();

            var types = GetClassesImplementingAnInterface(assemblies, typeof(IMessageHandler<>));

            foreach (var messageHandlerType in types)
            {
                // https://docs.microsoft.com/en-us/dotnet/api/system.type.getinterface
                var messageType = messageHandlerType.GetInterface(typeof(IMessageHandler<>).Name).GetGenericArguments()[0];

                var topics = messageHandlerType.GetCustomAttribute<TopicsAttribute>()?.Topics 
                    ?? throw new Exception($"{nameof(TopicsAttribute)} does not defined for MessageHandler : {messageHandlerType.Name}");

                subscriberBinding.RegisterTopicHandler(messageHandlerType, messageType, topics);
                
                services.AddScoped(messageHandlerType);
            }

            services.AddSingleton(subscriberBinding);
            services.AddSingleton<ISubscriberHandler, GenericSubscriberHandler>();

            return services;
        }

        private static IList<Type> GetClassesImplementingAnInterface(Assembly[] assembliesToScan, Type implementedInterface)
        {
            // if (implementedInterface == null || !implementedInterface.IsInterface)
            //     return Tuple.Create(false, (IList<Type>)null);

            IEnumerable<Type> typesInTheAssembly;

            try
            {
                typesInTheAssembly = assembliesToScan
                    .Select(s => s.GetTypes())
                    .SelectMany(s => s);
            }
            catch (ReflectionTypeLoadException e)
            {
                typesInTheAssembly = e.Types.Where(t => t != null);
            }

            IList<Type> classesImplementingInterface = new List<Type>();

            // if the interface is a generic interface
            if (implementedInterface.IsGenericType)
            {
                foreach (var typeInTheAssembly in typesInTheAssembly)
                {
                    if (typeInTheAssembly.IsClass)
                    {
                        var typeInterfaces = typeInTheAssembly.GetInterfaces();
                        foreach (var typeInterface in typeInterfaces)
                        {
                            if (typeInterface.IsGenericType)
                            {
                                var typeGenericInterface = typeInterface.GetGenericTypeDefinition();
                                var implementedGenericInterface = implementedInterface.GetGenericTypeDefinition();

                                if (typeGenericInterface == implementedGenericInterface)
                                {
                                    classesImplementingInterface.Add(typeInTheAssembly);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var typeInTheAssembly in typesInTheAssembly)
                {
                    if (typeInTheAssembly.IsClass)
                    {
                        // if the interface is a non-generic interface
                        if (implementedInterface.IsAssignableFrom(typeInTheAssembly))
                        {
                            classesImplementingInterface.Add(typeInTheAssembly);
                        }
                    }
                }
            }
            return classesImplementingInterface;
        }

    }


}