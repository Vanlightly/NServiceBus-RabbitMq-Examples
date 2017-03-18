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
    public class CancelOrderHandler : IHandleMessages<CancelOrder>
    {
        static ILog logger = LogManager.GetLogger<CancelOrderHandler>();

        public Task Handle(CancelOrder message, IMessageHandlerContext context)
        {
            logger.Info($"Received CancelOrder, OrderId = {message.OrderId}");

            // This is normally where some business logic would occur

            var orderCancelled = new OrderCancelled()
            {
                OrderId = message.OrderId,
                ClientId = message.ClientId
            };

            var publishOptions = new PublishOptions();
            publishOptions.SetHeader("insider", GetInsiderProgramValue(message));
            publishOptions.SetHeader("membership", GetMembership(message));

            return context.Publish(orderCancelled, publishOptions);
        }

        private string GetInsiderProgramValue(CancelOrder cancelOrder)
        {
            // get some data from a database or something

            if (cancelOrder.ClientId.Equals("SuperImportantClientLtd"))
                return "1";

            return "0";
        }

        private string GetMembership(CancelOrder cancelOrder)
        {
            // get some data from a database or something

            if (cancelOrder.ClientId.Equals("SuperImportantClientLtd") || cancelOrder.ClientId.Equals("AnotherSuperImportantClientLtd"))
                return "gold";

            return "silver";
        }
    }
}
