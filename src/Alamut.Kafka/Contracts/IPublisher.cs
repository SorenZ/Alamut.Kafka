﻿using System.Threading.Tasks;
using Alamut.Kafka.Models;

namespace Alamut.Kafka.Contracts
{
    public interface IPublisher
    {
         Task Publish(string topic, Message message);
    }
}