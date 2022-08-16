using System.Reflection;
using InventoryService;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configure Mass Transit
builder.Services.AddMassTransit(configure =>
{
    //Configure All Consumers in Assembly at once 
    var entryAssembly = Assembly.GetEntryAssembly();
    configure.AddConsumers(entryAssembly); 
    configure.AddSagaStateMachines(entryAssembly);
    configure.AddSagas(entryAssembly);
    configure.AddActivities(entryAssembly);

    //Configure every Consumers Manually
    //configure.AddConsumer<OrderConsumer>();

    configure.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("amqp://guest:guest@localhost:5672");
        
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();