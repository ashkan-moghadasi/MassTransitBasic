using MassTransit;
using Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configure =>
{
    configure.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(new Uri("amqp://guest:guest@localhost:5672"));
        cfg.ConfigureEndpoints(ctx);
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

app.Run();
