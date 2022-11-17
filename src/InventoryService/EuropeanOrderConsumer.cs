using MassTransit;
using Model;
using RabbitMQ.Client;

namespace InventoryService;

public class EuropeanOrderConsumer : IConsumer<Order>
{
    public Task Consume(ConsumeContext<Order> context)
    {
        if (!context.Message.IsValid)
        {
            throw new InvalidOperationException("Order Is Not Valid");
        }

        Console.Out.WriteLineAsync("European Order "+ context.Message.Name);
        return Task.CompletedTask;
    }
}
public class EuropeanOrderConsumerDefinition : ConsumerDefinition<EuropeanOrderConsumer>
{
    public EuropeanOrderConsumerDefinition()
    {
        // override the default endpoint name
        EndpointName = "order-queue-EU";

        // limit the number of messages consumed concurrently
        // this applies to the consumer only, not the endpoint
        ConcurrentMessageLimit = 8;

        /*Endpoint(cfg =>
        {
            cfg.Name = "order-queue";
            // set if each service instance should have its own endpoint for the consumer
            // so that messages fan out to each instance.
            cfg.InstanceId = "SomethingUnique";

        });*/


    }
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<EuropeanOrderConsumer> consumerConfigurator)
    {
        endpointConfigurator.ConfigureConsumeTopology = false;
        consumerConfigurator.UseMessageRetry(r => r.Interval(5, 1000));
        if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
        {
            rmq.Bind<Order>(x =>
            {
                x.RoutingKey = "192.168.13.*";
                x.ExchangeType = ExchangeType.Topic;
            });
        }
    }
}