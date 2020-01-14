using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alamut.Abstractions.Messaging;

namespace Alamut.Kafka.DependencyInjection
{
    public static class KafkaHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembliesToScan"></param>
        /// <returns></returns>
        public static List<string> GetAllTopics(params Assembly[] assembliesToScan)
        {
            List<string> topics = new List<string>();    
            var typesInTheAssembly = GetAllTypesInAssemblies(assembliesToScan);
            
            foreach (var typeInTheAssembly in typesInTheAssembly)
                {
                    if (typeInTheAssembly.IsClass)
                    {
                        var attributes = typeInTheAssembly.GetCustomAttributes<TopicsAttribute>(false);
                        foreach (var attribute in attributes)
                        { topics.AddRange(attribute.Topics);}
                    }
                }

            return topics;
        }

        public static IList<Type> GetClassesImplementingAnInterface(Assembly[] assembliesToScan, Type implementedInterface)
        {
            IEnumerable<Type> typesInTheAssembly = GetAllTypesInAssemblies(assembliesToScan);

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

        private static IEnumerable<Type> GetAllTypesInAssemblies(Assembly[] assembliesToScan)
        {
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

            return typesInTheAssembly;
        }

    }
}