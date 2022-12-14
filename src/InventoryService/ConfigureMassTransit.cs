using System.Reflection;
using MassTransit;
using Model;
using RabbitMQ.Client;

namespace InventoryService;

public static class ConfigureMassTransit
{
    public static IServiceCollection ConfigMassTransitTemporaryQueue(this IServiceCollection services)
    {
        services.AddMassTransit(configurator =>
        {
            //x.SetKebabCaseEndpointNameFormatter();
            services.AddHealthChecks();
            configurator.AddConsumer<EuropeanOrderConsumer, EuropeanOrderConsumerDefinition>()
                .Endpoint(e =>
                {
                    e.Temporary=true;
                    e.InstanceId = "62";
                });
            configurator.AddConsumer<AsianOrderConsumer, AsianOrderConsumerDefinition>()
                .Endpoint(e =>
                {
                    e.Temporary = true;
                    e.InstanceId = "63";
                    
                });
            

            configurator.UsingRabbitMq((context, cfg) => cfg.ConfigureEndpoints(context));
        });
        return services;
    }
    public static IServiceCollection ConfigMassTransitCustomRabbitMq(this IServiceCollection services)
    {
        services.AddMassTransit(configure =>
        {
            //Configure All Consumers in Assembly at once 
            var entryAssembly = Assembly.GetEntryAssembly();
            configure.AddConsumers(entryAssembly);
            configure.AddSagaStateMachines(entryAssembly);
            configure.AddSagas(entryAssembly);
            configure.AddActivities(entryAssembly);

            //Configure every Consumers Manually
            //configure.AddConsumer<OrderConsumer,OrderConsumerDefinition>();

            configure.UsingRabbitMq((ctx, cfg) =>
            {
                //cfg.Host("amqp://guest:guest@localhost:5672");
                cfg.Host(host:"localhost",virtualHost:"/", rabitcfg =>
                {
                    rabitcfg.Username("guest");
                    rabitcfg.Password("guest");
                    rabitcfg.PublisherConfirmation = true;
                });
                

                /*
                 Manually Configure each End Point(Queue)
                  cfg.ReceiveEndpoint("order-queue", c =>
                    {
                        c.ConfigureConsumer<OrderConsumer>(ctx);
                    });
                */

                //Configure All Endpoints at once
                cfg.ConfigureEndpoints(ctx);
            });


        });

        // OPTIONAL, but can be used to configure the bus options
        services.AddOptions<MassTransitHostOptions>()
            .Configure(options =>
            {
                // if specified, waits until the bus is started before
                // returning from IHostedService.StartAsync
                // default is false
                options.WaitUntilStarted = true;

                // if specified, limits the wait time when starting the bus
                options.StartTimeout = TimeSpan.FromSeconds(10);

                // if specified, limits the wait time when stopping the bus
                options.StopTimeout = TimeSpan.FromSeconds(30);
            });
        return services;

    }
    public static IServiceCollection ConfigureHeaderExchangeWithMassTransit(this IServiceCollection services)
    {
        var busControl = Bus.Factory.CreateUsingRabbitMq((cfg) =>
        {
            cfg.Host(host: "localhost", virtualHost: "/", rabitcfg =>
            {
                rabitcfg.Username("guest");
                rabitcfg.Password("guest");
                rabitcfg.PublisherConfirmation = true;
            });
            cfg.Publish<Order>(c =>
                c.ExchangeType = ExchangeType.Headers);

            cfg.ReceiveEndpoint("order-queue-AF", e =>
            {
                e.ConfigureConsumeTopology = false;
                e.Lazy = true;
                e.PrefetchCount = 20;

                e.Bind<Order>(x =>
                {
                    x.ExchangeType = ExchangeType.Headers;
                    x.SetBindingArgument("region", "Africa");

                });
                e.Consumer<GeneralOrderConsumer>();
            });
            cfg.ReceiveEndpoint("order-queue-EU", e =>
            {
                e.ConfigureConsumeTopology = false;
                e.Lazy = true;
                e.PrefetchCount = 20;

                e.Bind<Order>(x =>
                {
                    x.ExchangeType = ExchangeType.Headers;
                    x.SetBindingArgument("region", "Europe");

                });
                e.Consumer<GeneralOrderConsumer>();
            });
            cfg.ReceiveEndpoint("order-queue-AS", e =>
            {
                e.ConfigureConsumeTopology = false;
                e.Lazy = true;
                e.PrefetchCount = 20;

                e.Bind<Order>(x =>
                {
                    x.ExchangeType = ExchangeType.Headers;
                    x.SetBindingArgument("region", "Asian");

                });
                e.Consumer<GeneralOrderConsumer>();
            });
        });
        //30 Second wait for rabbitmq  
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        busControl.StartAsync(cancellation.Token);

        return services;
    }
}