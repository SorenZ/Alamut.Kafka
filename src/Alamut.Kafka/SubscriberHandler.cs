using System;
using System.Collections.Generic;

namespace Alamut.Kafka
{
    public class SubscriberHandler
    {
        public IDictionary<string, Type> TopicHandlers {get;set;} = new Dictionary<string,Type>();

        public SubscriberHandler RegisterTopicHandler<TSubscriber>(string topic)
        {
            if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }

            TopicHandlers[topic] = typeof(TSubscriber);

            return this;
        }
    }
}