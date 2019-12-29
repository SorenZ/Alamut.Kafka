# Alamut.Kafka  
## The purpose of this library is to easy to use Apache.Kafka in Dotnet (and ASP.NET) application on top of Dotnet Core Dependency Injection infrastructure.
## actually it's a wrapper on [Confluent's Apache Kafka .NET client](https://github.com/confluentinc/confluent-kafka-dotnet)

***

### Installing Alamut.Kafka
You should install [Alamut.Kafka with NuGet](https://www.nuget.org/packages/Alamut.Kafka):

    Install-Package Alamut.Kafka
    
Or via the .NET Core command line interface:

    dotnet add package Alamut.Kafka

Either commands, from Package Manager Console or .NET Core CLI, will download and install all required dependencies.

***

### Basic Configuration
These simple basic settings are needed to communicate with Kafka
```js
"KafkaConfig": {
    "BootstrapServers": "10.104.51.12:9092,10.104.51.13:9092,10.104.51.14:9092",
    "GroupId": "alamut.group",
    "Topics": ["alamut-soft"]
  }
```
You have to inject configuration as a [KafkaConfig](https://github.com/SorenZ/Alamut.Kafka/blob/master/src/Alamut.Kafka/Models/KafkaConfig.cs) into your DI :
```csharp
services.AddPoco<KafkaConfig>(Configuration);
```
*[AddPoco](https://github.com/SorenZ/Alamut.AspNet/wiki/Add-POCO) is an Alamut Helper*

[comprehensive samples](https://github.com/SorenZ/Alamut.Kafka/blob/master/samples/Alamut.Kafka.Consumer/Startup.cs)

### Producer
Producer publish a message into specified topic.
We ususally use [IPublisher](https://github.com/SorenZ/Alamut.Abstractions/blob/master/src/Alamut.Abstractions/Messaging/IPublisher.cs) as a producer in our application.
The publisher could publish a message in a variety types of data structure:
* String (use native Kafka Client's serializer) 
* Object (serialize it to JSON)
* [IMessage](https://github.com/SorenZ/Alamut.Abstractions/blob/master/src/Alamut.Abstractions/Messaging/IMessage.cs) 

**Producer Sample**


**Setup Producer**
