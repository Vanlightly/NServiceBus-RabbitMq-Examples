using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.CustomRouting.GoldInsiderClientServices
{
    class Program
    {
        static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            Console.Title = "Rabbit Custom Gold Insider Client Services";

            var endpointConfiguration = new EndpointConfiguration("Rabbit.CustomRouting.GoldInsiderClientServices");

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=localhost");
            transport.UsePublisherConfirms(true);

            transport.UseRoutingTopology(
                topologyFactory: createDurableExchangesAndQueues =>
                {
                    return new ClientServicesRoutingTopology(createDurableExchangesAndQueues);
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
