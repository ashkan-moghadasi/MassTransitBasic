using MassTransit;
using Model;

namespace InventoryService;

public class OrderConsumer : IConsumer<Order>
{
    public  Task Consume(ConsumeContext<Order> context)
    {
         Console.Out.WriteLineAsync(context.Message.Name);
         return Task.CompletedTask;

    }
}

public class OrderConsumerDefinition:ConsumerDefinition<OrderConsumer>
{
    public OrderConsumerDefinition()
    {
        // override the default endpoint name
        EndpointName = "order-queue";

        // limit the number of messages consumed concurrently
        // this applies to the consumer only, not the endpoint
        ConcurrentMessageLimit = 8;

        /*Endpoint(cfg=>
        {
            cfg.Name = "HelloMessage-Queue";
            // set if each service instance should have its own endpoint for the consumer
            // so that messages fan out to each instance.
            cfg.InstanceId = "SomethingUnique";
        });*/

    }
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<OrderConsumer> consumerConfigurator)
    {
        base.ConfigureConsumer(endpointConfigurator, consumerConfigurator);
        consumerConfigurator.UseMessageRetry(r=>r.Interval(5,1000));
    }
}