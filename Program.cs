using System;
using System.Threading.Tasks;

using Company.Consumers;

using Contracts;

using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MassTransitRawBatchConsumer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var awsRegion = hostContext.Configuration.GetValue<string>("aws:region");
                    services.AddMassTransit(configure =>
                    {
                        configure.SetKebabCaseEndpointNameFormatter();

                        configure.AddDelayedMessageScheduler();

                        configure.AddConsumer(typeof(TestConsumer));
                        configure.UsingAmazonSqs((cntxt, cfg) =>
                        {
                            cfg.Host(awsRegion, _ => { });
                            cfg.UseNewtonsoftJsonSerializer();

                            cfg.ReceiveEndpoint("test-queue", ec =>  //queue name
                            {
                                ec.PrefetchCount = 5;

                                ec.Batch<TestContract>(x =>
                                {
                                    x.ConcurrencyLimit = 3;
                                    x.MessageLimit = 5;
                                    x.TimeLimit = TimeSpan.FromSeconds(1);

                                    x.Consumer<TestConsumer, TestContract>(cntxt);
                                });
                                //disable the default topic binding
                                ec.ConfigureConsumeTopology = false;

                                ec.ClearSerialization();
                                ec.UseNewtonsoftRawJsonSerializer();
                            });
                        });
                    });
                });
    }
}
