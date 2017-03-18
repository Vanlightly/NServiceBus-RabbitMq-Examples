using Messages;
using Messages.Commands;
using Messages.Events;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.CustomRouting.Sales.CommandHandlers
{
    public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        static ILog logger = LogManager.GetLogger<PlaceOrderHandler>();

        public Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            logger.Info($"Received PlaceOrder, OrderId = {message.OrderId}");

            // This is normally where some business logic would occur
            
            var orderPlaced = new OrderPlaced
            {
                OrderId = message.OrderId,
                ClientId = message.ClientId
            };

            var publishOptions = new PublishOptions();
            publishOptions.SetHeader("insider", GetInsiderProgramValue(message));
            publishOptions.SetHeader("membership", GetMembership(message));

            return context.Publish(orderPlaced, publishOptions);
        }

        private string GetInsiderProgramValue(PlaceOrder placeOrder)
        {
            // get some data from a database or something

            if (placeOrder.ClientId.Equals("SuperImportantClientLtd"))
                return "1";

            return "0";
        }

        private string GetMembership(PlaceOrder placeOrder)
        {
            // get some data from a database or something

            if (placeOrder.ClientId.Equals("SuperImportantClientLtd") || placeOrder.ClientId.Equals("AnotherSuperImportantClientLtd"))
                return "gold";

            return "silver";
        }
    }
}
