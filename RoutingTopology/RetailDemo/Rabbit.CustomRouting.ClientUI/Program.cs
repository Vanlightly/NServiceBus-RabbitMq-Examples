using Messages.Commands;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.CustomRouting.ClientUI
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
            Console.Title = "Rabbit Custom Routing ClientUI";

            var endpointConfiguration = new EndpointConfiguration("Rabbit.CustomRouting.ClientUI");

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=localhost");
            transport.UsePublisherConfirms(true);

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(PlaceOrder), "Rabbit.CustomRouting.Sales");
            routing.RouteToEndpoint(typeof(CancelOrder), "Rabbit.CustomRouting.Sales");

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
                log.Info("Press 'Numpad 1' to place an order by a Gold Insider client");
                log.Info("Press 'Numpad 2' to place an order by a Gold Standard client");
                log.Info("Press 'Numpad 3' to place an order by a Silver client");
                log.Info("Press 'Numpad 4' to cancel an order by a Gold Insider client");
                log.Info("Press 'Numpad 5' to cancel an order by a Gold Standard client");
                log.Info("Press 'Numpad 6' to cancel an order by a Silver client");
                log.Info("Press 'Q' to quit.");
                var key = Console.ReadKey();
                Console.WriteLine();

                switch (key.Key)
                {
                    case ConsoleKey.NumPad1:
                        var command = new PlaceOrder { OrderId = Guid.NewGuid().ToString(), ClientId = "SuperImportantClientLtd" };
                        log.Info($"Sending Gold Insider PlaceOrder command, OrderId = {command.OrderId}");
                        await endpointInstance.Send(command).ConfigureAwait(false);
                        break;

                    case ConsoleKey.NumPad2:
                        var command2 = new PlaceOrder { OrderId = Guid.NewGuid().ToString(), ClientId = "AnotherSuperImportantClientLtd" };
                        log.Info($"Sending Gold Standard PlaceOrder command, OrderId = {command2.OrderId}");
                        await endpointInstance.Send(command2).ConfigureAwait(false);
                        break;

                    case ConsoleKey.NumPad3:
                        var command3 = new PlaceOrder { OrderId = Guid.NewGuid().ToString(), ClientId = "NotSoImportantClient" };
                        log.Info($"Sending Silver PlaceOrder command, OrderId = {command3.OrderId}");
                        await endpointInstance.Send(command3).ConfigureAwait(false);
                        break;

                    case ConsoleKey.NumPad4:
                        var command4 = new CancelOrder { OrderId = Guid.NewGuid().ToString(), ClientId = "SuperImportantClientLtd" };
                        log.Info($"Sending Gold Insider CancelOrder command, OrderId = {command4.OrderId}");
                        await endpointInstance.Send(command4).ConfigureAwait(false);
                        break;

                    case ConsoleKey.NumPad5:
                        var command5 = new CancelOrder { OrderId = Guid.NewGuid().ToString(), ClientId = "AnotherSuperImportantClientLtd" };
                        log.Info($"Sending Gold Standard CancelOrder command, OrderId = {command5.OrderId}");
                        await endpointInstance.Send(command5).ConfigureAwait(false);
                        break;

                    case ConsoleKey.NumPad6:
                        var command6 = new CancelOrder { OrderId = Guid.NewGuid().ToString(), ClientId = "NotSoImportantClient" };
                        log.Info($"Sending Silver PlaceOrder command, OrderId = {command6.OrderId}");
                        await endpointInstance.Send(command6).ConfigureAwait(false);
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
