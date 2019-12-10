using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alamut.AspNet.Configuration;
using Alamut.Kafka.Consumer.Subscribers;
using Alamut.Kafka.Models;
using Kafka.Consumer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alamut.Kafka.Consumer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddPoco<KafkaConfig>(Configuration);
            services.AddHostedService<KafkaService>();
            services.AddSingleton(_ => 
            {
                return new SubscriberHandler()
                    .RegisterTopicHandler<SendSms>("mobin-soft");
                    // .RegisterTopicHandler
            });
            services.AddScoped<SendSms>();

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
