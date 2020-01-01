using System;

namespace Alamut.Kafka.DependencyInjection
{
    /// <summary>
    /// binds a MessageHandler to specific topic(s)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TopicsAttribute : Attribute
    {
        public string[] Topics { get; }

        public TopicsAttribute(params string[] topics)
        {
            Topics = topics;
        }
    }
}