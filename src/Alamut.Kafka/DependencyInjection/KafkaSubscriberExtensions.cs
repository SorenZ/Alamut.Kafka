using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alamut.Abstractions.Messaging;
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


                return new KafkaSubscriber(
                    provider.GetRequiredService<ILoggerFactory>(),
                    config,
                    provider.GetRequiredService<ISubscriberHandler>());
            });
        }

        public static IServiceCollection RegisterGenericSubscriberHandler(this IServiceCollection services)
            => services.AddSingleton<ISubscriberHandler, GenericSubscriberHandler>();

        public static IServiceCollection RegisterMessageHandlers(this IServiceCollection services, params Assembly[] assemblies)
        {
            var subscriberBinding = new SubscriberBinding();

            // var iMessageHandlerType = typeof(IMessageHandler<>);
            // var types = assemblies
            //     .SelectMany(s => s.GetTypes())
            //     .Where(p => iMessageHandlerType.IsAssignableFrom(p) && p.IsClass);

            var types = GetClassesImplementingAnInterface(assemblies[0], typeof(IMessageHandler<>));

            foreach (var messageHandlerType in types)
            {
                var messageType = messageHandlerType.GetInterfaces()[0].GetGenericArguments()[0];
                var topics = new []{"mobin-net"};

                subscriberBinding.RegisterTopicHandler(messageHandlerType, messageType, topics);
                
                services.AddScoped(messageHandlerType);

            }

            services.AddSingleton(subscriberBinding);

            return services;
        }

        public static IList<Type> GetClassesImplementingAnInterface(Assembly assemblyToScan, Type implementedInterface)
        {
            // if (implementedInterface == null || !implementedInterface.IsInterface)
            //     return Tuple.Create(false, (IList<Type>)null);

            IEnumerable<Type> typesInTheAssembly;

            try
            {
                typesInTheAssembly = assemblyToScan.GetTypes();
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