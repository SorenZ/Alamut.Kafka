using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alamut.Abstractions.Messaging;
using Alamut.AspNet.Configuration;
using Alamut.Kafka.Consumer.Subscribers;
using Alamut.Kafka.DependencyInjection;
using Alamut.Kafka.Models;
using Alamut.Kafka.SubscriberHandlers;
using Confluent.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alamut.Kafka.Consumer
{
    public class Startup
    {
        public const string TestTopic = "mobin-net";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // --------------<( configuration )>---------------------------- 
            services.AddPoco<ConsumerConfig>(Configuration, "KafkaConfig");
            services.AddPoco<ProducerConfig>(Configuration, "KafkaConfig");
            
            // --------------<( register publisher )>---------------------------- 
            services.AddScoped<IPublisher,KafkaProducer>();

            // --------------<( register handlers  )>---------------------------- 
            var registeredTopics = services.RegisterMessageHandlers(typeof(SendSmsGeneric).Assembly);

            // --------------<( register hosted service  )>---------------------------- 
            /* 1- */ services.AddNewHostedSubscriber(registeredTopics); 
            /* 2- */ //services.AddHostedSubscriber();
            
            // --------------<( register hosted service)>---------------------------- 
            // services.AddHostedService<KafkaSubscriber>();
            // services.AddNewHostedSubscriber("mobin-net");
            // services.AddNewHostedSubscriber(KafkaHelper.GetAllTopics(typeof(SendSmsGeneric).Assembly));
            //services.AddNewHostedSubscriber("group2","mobin-net");
            //services.AddNewHostedSubscriber("group3","mobin-net");
            
            

            // --------------<( string handler)>---------------------------- 
            // services.AddSingleton<ISubscriberHandler, StringSubscriberHandler>();
            // services.AddSingleton(_ =>
            //    new SubscriberBinding()
            //        .RegisterTopicHandler<SendSms>("mobin-soft"));
            // services.AddScoped<SendSms>();
            // -----------------------------------------------------------------

            // --------------<( dynamic handler)>---------------------------- 
            // services.AddSingleton<ISubscriberHandler,DynamicSubscriberHandler>();
            // services.AddSingleton(_ => 
            //     new SubscriberBinding()
            //         .RegisterTopicHandler<SendSmsDynamic>("mobin-soft"));
            // services.AddScoped<SendSmsDynamic>();
            // -----------------------------------------------------------------

            // --------------<( JObject handler)>---------------------------- 
            // services.AddSingleton<ISubscriberHandler,JObjectSubscriberHandler>();
            // services.AddSingleton(_ => 
            //     new SubscriberBinding()
            //         .RegisterTopicHandler<SendSmsJObject>("mobin-soft"));
            // services.AddScoped<SendSmsJObject>();
            // -----------------------------------------------------------------

            // --------------<( Generic handler)>---------------------------- 
            // services.AddSingleton<ISubscriberHandler, GenericSubscriberHandler>();
            
            // services.AddSingleton(_ => new SubscriberBinding()
            //     .RegisterTopicHandler<SendSmsGeneric,FooMessage>("mobin-net"));
            // services.AddScoped<SendSmsGeneric>();
            // -----------------------------------------------------------------

            // --------------<( Generic handler - DI Helpers)>------------------
            // services.RegisterMessageHandlers<SendSmsGeneric>();
            // -----------------------------------------------------------------

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
