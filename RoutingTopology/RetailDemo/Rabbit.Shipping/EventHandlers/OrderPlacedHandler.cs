using Messages.Events;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Shipping.EventHandlers
{
    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        static ILog logger = LogManager.GetLogger<OrderPlacedHandler>();

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            logger.Info($"Received OrderPlaced, OrderId = {message.OrderId} - preparing for shipment");
            return Task.CompletedTask;
        }
    }
}
