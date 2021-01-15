using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using CrystalQuartz.AspNetCore;
using GreenPipes;
using MassTransit;
using MassTransit.QuartzIntegration;
using MassTransit.Scheduling;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using Quartzmin;
using QuartzService.Options;

namespace QuartzService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RabbitMqOptions>(Configuration.GetSection("RabbitMq"));

            services.AddSingleton(x => new StdSchedulerFactory().GetScheduler().ConfigureAwait(false).GetAwaiter().GetResult());

            // Service Bus
            services.AddMassTransit(mt =>
            {
                mt.UsingRabbitMq((context, cfg) =>
                {
                    var scheduler = context.GetRequiredService<IScheduler>();
                    var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                    cfg.Host(options.Host, options.VirtualHost, h =>
                    {
                        h.Username(options.Username);
                        h.Password(options.Password);
                    });

                    cfg.UseJsonSerializer(); // json as Quartz serializer type

                    cfg.ReceiveEndpoint(options.SchedulerQueueName, endpoint =>
                    {
                        var partitionCount = Environment.ProcessorCount;
                        endpoint.PrefetchCount = (ushort)(partitionCount);
                        var partitioner = endpoint.CreatePartitioner(partitionCount);

                        endpoint.Consumer(() => new ScheduleMessageConsumer(scheduler), x =>
                            x.Message<ScheduleMessage>(m => m.UsePartitioner(partitioner, p => p.Message.CorrelationId)));
                        endpoint.Consumer(() => new CancelScheduledMessageConsumer(scheduler),
                            x => x.Message<CancelScheduledMessage>(m => m.UsePartitioner(partitioner, p => p.Message.TokenId)));
                    });
                });
            });

            services.AddHostedService<MassTransitHostedService>();

            services.AddHealthChecks();
            services.AddControllers(o => { o.EnableEndpointRouting = false; });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthorization();

            var scheduler = app.ApplicationServices.GetRequiredService<IScheduler>();

            // remove if you don't want to use management UI: UseCrystalQuartz
            app.UseCrystalQuartz(() => scheduler);

            // remove if you don't want to use management UI: Quartzmin
            app.UseQuartzmin(new QuartzminOptions()
            {
                Scheduler = scheduler,
                ProductName = "QuartzForMassTransit"
            });

            app.UseHealthChecks("/healthCheck");
        }
    }
}
