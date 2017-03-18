using Messages.Events;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Custom.Sales
{
    class Program
    {
        static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            Console.Title = "Rabbit Custom Sales";

            var endpointConfiguration = new EndpointConfiguration("Rabbit.Custom.Sales");

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=localhost");
            transport.UsePublisherConfirms(true);
            transport.UseDirectRoutingTopology(
                routingKeyConvention: (eventType) => 
                {
                    if (eventType.UnderlyingSystemType.Equals(typeof(OrderPlaced))) // for publishing
                        return "Order.Placed";
                    else if (eventType.UnderlyingSystemType.Equals(typeof(OrderCancelled))) // for publishing
                        return "Order.Cancelled";
                    else
                        return "Order.Unroutable";
                },
                exchangeNameConvention: (address, eventType) =>
                {
                    return "Orders";
                });

            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.EnableInstallers();

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await endpointInstance.Stop().ConfigureAwait(false);
        }
    }
}


