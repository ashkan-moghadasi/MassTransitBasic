using MassTransit;
using Model;
using RabbitMQ.Client;

namespace InventoryService;

public class AfricanOrderConsumer : IConsumer<Order>
{
    public Task Consume(ConsumeContext<Order> context)
    {
        if (!context.Message.IsValid)
        {
            throw new InvalidOperationException("Order Is Not Valid");
        }

        Console.Out.WriteLineAsync("African Order "+ context.Message.Name);
        return Task.CompletedTask;
    }
}