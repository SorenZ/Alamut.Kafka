# Alamut.Kafka  
The purpose of this library is to easy to use Apache.Kafka in Dotnet (Console, ASP.NET, Web API) application on top of Dotnet Core Dependency Injection infrastructure.  
Actually it's a wrapper of [Confluent's Apache Kafka .NET client](https://github.com/confluentinc/confluent-kafka-dotnet)

***
*Documentation is based on [v0.8.1](https://github.com/SorenZ/Alamut.Kafka/tree/v0.8.1)*
***

### Installing Alamut.Kafka
You should install [Alamut.Kafka with NuGet](https://www.nuget.org/packages/Alamut.Kafka):

    Install-Package Alamut.Kafka
    
Or via the .NET Core command-line interface:

    dotnet add package Alamut.Kafka

Either commands, from Package Manager Console or .NET Core CLI, will download and install all required dependencies.

***

### Basic Configuration
These simple basic settings are needed to communicate with Kafka  
*This configuration will be used for both `ProducerConfig` and `ConsumerConfig` *
```js
"KafkaConfig": {
    "BootstrapServers": "10.104.51.12:9092,10.104.51.13:9092,10.104.51.14:9092",
    "GroupId": "alamut.group",
    // All Producer and Consumer configuration
  }
```

You have to inject configuration into your DI :
```csharp
// for Consumer configuration
services.AddPoco<ConsumerConfig>(Configuration, "KafkaConfig");
// for Producer configuration
services.AddPoco<ProducerConfig>(Configuration, "KafkaConfig");
```
*[AddPoco](https://github.com/SorenZ/Alamut.AspNet/wiki/Add-POCO) is an Alamut Helper*

[comprehensive samples](https://github.com/SorenZ/Alamut.Kafka/blob/master/samples/Alamut.Kafka.Consumer/Startup.cs)

***

### Producer
Producer publish a message into specified topic.
We ususally use [IPublisher](https://github.com/SorenZ/Alamut.Abstractions/blob/master/src/Alamut.Abstractions/Messaging/IPublisher.cs) as a producer in our application.
The publisher could publish a message in a variety types of data structure:
* String (use native Kafka Client's serializer) 
* Object (serialize it to JSON)
* [IMessage](https://github.com/SorenZ/Alamut.Abstractions/blob/master/src/Alamut.Abstractions/Messaging/MessageContracts/IMessage.cs) 

**Producer Sample**
```csharp
IPublisher publisher = new KafkaProducer(*/dependencies provided by DI*/);

// string message
await publisher.Publish("alamut.messaging.kafka", "a string message");

// object message
var objectMessage = new Foo
{
    Bar = message
};
await publisher.Publish("alamut.messaging.kafka", objectMessage);

// IMessage message
var typedMessage = new Foo
{
    Bar = message
};
await publisher.Publish("alamut.messaging.kafka", MessageFactory.Build(typedMessage));
```
*we will talk about the [MessageFactory](https://github.com/SorenZ/Alamut.Abstractions/blob/master/src/Alamut.Abstractions/Messaging/MessageFactory.cs) in more details latter.*

**Register Producer**  
If you want to get IPublisher through DI you should register it in project Startup:  
```csharp
services.AddSingleton<IPublisher, KafkaProducer>();
```

***

### Consumer  
Consumer subscribes to the specified topic(s) and works as a [Background Hosted Service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services).  

Consumer automatically calls the classes that implemented [IMessageHandler<TMessage>](https://github.com/SorenZ/Alamut.Abstractions/blob/master/src/Alamut.Abstractions/Messaging/IMessageHandler%5BTMessage%5D.cs) interface that decorated with [TopicsAttribute](https://github.com/SorenZ/Alamut.Abstractions/blob/master/src/Alamut.Abstractions/Messaging/TopicsAttribute.cs).  

**Message Handler Sample:**
```csharp
using System.Threading;
using System.Threading.Tasks;

using Alamut.Abstractions.Messaging;
using Alamut.Kafka.Models;
using Microsoft.Extensions.Logging;

namespace Alamut.Kafka.Consumer.Subscribers
{
    [Topics("alamut.messaging.kafka")]
    public class SendSmsGeneric : IMessageHandler<Message<Foo>>
    {
        private readonly ILogger _logger;

        public SendSmsGeneric(ILogger<SendSmsGeneric> logger)
        {
            _logger = logger;

        }
        public Task Handle(Message<Foo> message, CancellationToken token)
        {
            _logger.LogInformation($"Received message <{ message.Body.Bar }>");

            return Task.CompletedTask;
        }
    }
}
```
In the example above [SendSmsGeneric](https://github.com/SorenZ/Alamut.Kafka/blob/master/samples/Alamut.Kafka.Consumer/Subscribers/SendSmsGeneric.cs) handles `FooMessage` that published in `alamut.messaging.kafka` Topic. (other Message handlers have not yet documented)  

**Consumer Registration and Wiring**
* First of all, you need a simple configuration that described above.  
* Register Message Handlers:  
    `services.RegisterMessageHandlers(typeof(SendSmsGeneric).Assembly);`   
    registers all classes that implemented [IMessageHandler<TMessage>](https://github.com/SorenZ/Alamut.Abstractions/blob/master/src/Alamut.Abstractions/Messaging/IMessageHandler%5BTMessage%5D.cs) in the specified assembly. 
* Then you have to register your Hosted Service to subscribe to Kafka Messages, There are two ways:  
  * Register Hosted Service with default `GroupId` and specified `Topics` that discovered in `RegisterMessageHandlers` section:  
    `services.AddHostedSubscriber();`
  * Register Hosted Service for specifics Topic(s): 
      * `services.AddNewHostedSubscriber("alamut.messaging.kafka", ... );`  
        in this case, a Hosted Services registered and handles just provided Topic(s).  
      * `services.AddNewHostedSubscriber(KafkaHelper.GetAllTopics(typeof(SendSmsGeneric).Assembly));`  
        a Hosted Service registered and handles topics provide by [KafkaHelper.GetAllTopics](https://github.com/SorenZ/Alamut.Kafka/blob/master/src/Alamut.Kafka/DependencyInjection/KafkaHelper.cs#L16)   
       
 
With these two steps your wiring will be completed. ([example](https://github.com/SorenZ/Alamut.Kafka/blob/master/samples/Alamut.Kafka.Consumer/Startup.cs))  
*There are other ways to do this that will be explained later* 

