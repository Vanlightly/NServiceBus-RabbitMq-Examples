using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Sales.DelayInfrastructure
{
    class Program
    {
        static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            Console.Title = "Rabbit Sales";

            var endpointConfiguration = new EndpointConfiguration("Rabbit.Sales");

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=localhost");
            transport.UsePublisherConfirms(true);

            var delayedDelivery = transport.DelayedDelivery();
            delayedDelivery.DisableTimeoutManager();
            delayedDelivery.AllEndpointsSupportDelayedDelivery();

            // disabled immediate retries and set delayed retries with a one second increment
            // run the view_delays.sql script to see the delays in action
            var recoverability = endpointConfiguration.Recoverability();
            //recoverability.DisableLegacyRetriesSatellite();

            recoverability.Delayed(
                customizations: delayed =>
                {
                    delayed.NumberOfRetries(20);
                    delayed.TimeIncrease(TimeSpan.FromSeconds(1));
                });

            recoverability.Immediate(
                customizations: imm =>
                {
                    imm.NumberOfRetries(0);
                });

            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.UseSerialization<JsonSerializer>();
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
