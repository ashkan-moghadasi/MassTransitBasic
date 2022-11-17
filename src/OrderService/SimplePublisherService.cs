using MassTransit;
using MassTransit.Monitoring;
using Microsoft.Extensions.Hosting;
using Model;

namespace OrderService
{
    public class SimplePublisherService : BackgroundService
    {
        private readonly IBus _bus;
        private readonly IBusControl _busControl;

        public SimplePublisherService(IBus bus,IBusControl busControl)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _busControl = busControl ?? throw new ArgumentNullException(nameof(busControl));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _busControl.WaitForHealthStatus(BusHealthStatus.Healthy, stoppingToken);
            var sendEndpoint=_bus.GetSendEndpoint(new Uri("exchange:Model:Order")).Result;

            await _bus.Publish(new Order()
                    {
                        Name = "Single Order",
                        IsValid = true
                    },
                context =>
                        {
                            context.SetRoutingKey("AS");
                            
                        } 
                ,stoppingToken);
            
        }
    }
}
