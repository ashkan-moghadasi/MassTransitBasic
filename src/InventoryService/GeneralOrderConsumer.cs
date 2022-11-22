using MassTransit;
using Model;

namespace InventoryService;

public class GeneralOrderConsumer : IConsumer<Order>
{
    public  Task Consume(ConsumeContext<Order> context)
    {
        if (!context.Message.IsValid)
        {
            throw new InvalidOperationException("Order Is Not Valid");
        }
        var region=context.Message.Region;
        Console.Out.WriteLineAsync($"New {region} Order Received" + context.Message.Name);
        return Task.CompletedTask;
    }
}