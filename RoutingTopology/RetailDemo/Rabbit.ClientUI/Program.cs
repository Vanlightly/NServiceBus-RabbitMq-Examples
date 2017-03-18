using Messages.Commands;
using Messages.Events;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.ClientUI
{
    class Program
    {
        static ILog log = LogManager.GetLogger<Program>();

        static void Main(string[] args)
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            Console.Title = "Rabbit ClientUI";

            var endpointConfiguration = new EndpointConfiguration("Rabbit.ClientUI");

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=localhost");
            transport.UsePublisherConfirms(true);

            // comment this line for Conventional Routing Topology
            transport.UseDirectRoutingTopology();
            
            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(PlaceOrder), "Rabbit.Sales");
            routing.RouteToEndpoint(typeof(CancelOrder), "Rabbit.Sales");

            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.EnableInstallers();

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            await RunLoop(endpointInstance);

            await endpointInstance.Stop().ConfigureAwait(false);
        }

        static async Task RunLoop(IEndpointInstance endpointInstance)
        {
            while (true)
            {
                log.Info("Press '1' to place an order by a Gold Insider client");
                log.Info("Press '2' to place an order by a Gold Standard client");
                log.Info("Press '3' to place an order by a Silver client");
                log.Info("Press '4' to cancel an order by a Gold Insider client");
                log.Info("Press '5' to cancel an order by a Gold Standard client");
                log.Info("Press '6' to cancel an order by a Silver client");
                log.Info("Press 'Q' to quit.");
                var key = Console.ReadKey();
                Console.WriteLine();

                switch (key.Key)
                {
                    case ConsoleKey.NumPad1:
                        // Instantiate the command
                        var command = new PlaceOrder
                        {
                            OrderId = Guid.NewGuid().ToString()
                        };

                        // Send the command to the local endpoint
                        log.Info($"Sending PlaceOrder command, OrderId = {command.OrderId}");

                        var sendOptions = new SendOptions();
                        sendOptions.SetHeader("insider", "1");
                        sendOptions.SetHeader("membership", "gold");
                        await endpointInstance.Send(command, sendOptions).ConfigureAwait(false);

                        break;

                    case ConsoleKey.NumPad2:
                        // Instantiate the command
                        var command2 = new PlaceOrder
                        {
                            OrderId = Guid.NewGuid().ToString()
                        };

                        // Send the command to the local endpoint
                        log.Info($"Sending PlaceOrder command, OrderId = {command2.OrderId}");

                        var sendOptions2 = new SendOptions();
                        sendOptions2.SetHeader("insider", "0");
                        sendOptions2.SetHeader("membership", "gold");
                        await endpointInstance.Send(command2, sendOptions2).ConfigureAwait(false);

                        break;

                    case ConsoleKey.C:
                        //Instantiate the command
                        var cancelCommand = new CancelOrder
                        {
                            OrderId = Guid.NewGuid().ToString()
                        };

                        // Send the command to the local endpoint
                        log.Info($"Sending CancelOrder command, OrderId = {cancelCommand.OrderId}");
                        await endpointInstance.Send(cancelCommand).ConfigureAwait(false);

                        break;

                    case ConsoleKey.Q:
                        return;

                    default:
                        log.Info("Unknown input. Please try again.");
                        break;
                }
            }
        }
    }
}
