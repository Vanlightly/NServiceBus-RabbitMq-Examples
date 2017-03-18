using Messages.Events;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipping.EventHandlers
{
    public class OrderCancelledHandler : IHandleMessages<OrderCancelled>
    {
        static ILog logger = LogManager.GetLogger<OrderCancelledHandler>();

        public Task Handle(OrderCancelled message, IMessageHandlerContext context)
        {
            logger.Info($"Received OrderCancelled, OrderId = {message.OrderId} - shipment cancelled");
            return Task.CompletedTask;
        }
    }
}
