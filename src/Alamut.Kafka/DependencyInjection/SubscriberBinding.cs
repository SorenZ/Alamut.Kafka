using System;
using System.Collections.Generic;
using System.Linq;

using Alamut.Abstractions.Messaging;

namespace Alamut.Kafka
{
    /// <summary>
    /// responsible for holding a map between topics and types those should handle the topic 
    /// </summary>
    public class SubscriberBinding 
    {
        public IDictionary<string, Type> TopicHandlers { get; private set; } = new Dictionary<string, Type>();
        public IDictionary<string, KeyValuePair<Type,Type>> GenericTopicHandlers { get; internal set; }
            = new Dictionary<string, KeyValuePair<Type,Type>>();

        public SubscriberBinding RegisterTopicHandler<TSubscriber>(string topic)
        {
            if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }

            TopicHandlers[topic] = typeof(TSubscriber);

            return this;
        }

        /// <summary>
        /// registers a binding between MessageHandler and coresponding topic(s)
        /// </summary>
        /// <param name="topics">topic(s) to handle with provided MessageHandler type</param>
        /// <typeparam name="TMessageHandler">MessageHandler to handler provided topic(s)</typeparam>
        /// <typeparam name="TMessage">the type of Message should deserialized and pass to MessageHandler</typeparam>
        /// <returns></returns>
        public SubscriberBinding RegisterTopicHandler<TMessageHandler, TMessage>(params string[] topics)
            where TMessageHandler : IMessageHandler<TMessage>
        {
            if (topics == null || !topics.Any()) 
                { throw new ArgumentNullException("Topics were not provided for MessageHandler : " + typeof(TMessageHandler).Name); }

            foreach (var topic in topics)
            {
                this.GenericTopicHandlers[topic] = new KeyValuePair<Type, Type>(typeof(TMessageHandler),typeof(TMessage));
            }

            return this;
        }

        public SubscriberBinding RegisterTopicHandler(Type typeMessageHandler, Type typeMessage, params string[] topics)
        {
            if (topics == null || !topics.Any()) { throw new ArgumentNullException(nameof(topics)); }

            foreach (var topic in topics)
            {
                this.GenericTopicHandlers[topic] = new KeyValuePair<Type, Type>(typeMessageHandler,typeMessage);
            }

            return this;
        }
        
    }


}