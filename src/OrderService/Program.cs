using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Model;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Start Background Service
//builder.Services.AddHostedService<SimplePublisherService>();
//Connect To MassTransit
builder.Services.AddMassTransit(configure =>
{
    configure.UsingRabbitMq((ctx, cfg) =>
    {
        
        cfg.Host(new Uri("amqp://guest:guest@localhost:5672"));
        cfg.ConfigureEndpoints(ctx);
        cfg.Publish<Order>(c =>
            c.ExchangeType=ExchangeType.Headers);

    });
});


/*
 Config MassTransit Manually without AspNetCore Extension
 Use full For Service Projects
var bus=Bus.Factory.CreateUsingRabbitMq(config =>
{
    config.Host("amqp://guest:guest@localhost:5672");
    config.ReceiveEndpoint(queueName:"temp-queue", c =>
    {
        c.Handler<Order>(ctx =>
        {
            return Console.Out.WriteLineAsync(ctx.Message.Name);
        });
    });
    
});
bus.StartAsync();
bus.Publish(new Order() { Name = "Hello MassTransit" });
*/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();


app.MapPost("/order/send", async (IPublishEndpoint orderPublisher,[FromBody]Order order) =>
{
    
    await orderPublisher.Publish(order, context =>
    {
        //context.SetRoutingKey(order.Name);
        context.Headers.Set("region",order.Region);
    });
});

app.Run();
