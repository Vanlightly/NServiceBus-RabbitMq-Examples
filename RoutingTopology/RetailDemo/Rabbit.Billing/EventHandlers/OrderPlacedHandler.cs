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

namespace Rabbit.Billing.EventHandlers
{
    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        static ILog logger = LogManager.GetLogger<OrderPlacedHandler>();

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            logger.Info($"Received OrderPlaced, OrderId = {message.OrderId} - charging credit card");


            var orderBilled = new OrderBilled
            {
                OrderId = message.OrderId
            };
            return context.Publish(orderBilled);
        }
    }
}
