using System;
using System.Collections.Generic;

namespace Alamut.Kafka.SubscriberHandlers
{
    public class SubscriberBinding 
    {
        public IDictionary<string, Type> TopicHandlers { get; private set; } = new Dictionary<string, Type>();

        public SubscriberBinding RegisterTopicHandler<TSubscriber>(string topic)
        {
            if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }

            TopicHandlers[topic] = typeof(TSubscriber);

            return this;
        }
    }


}