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
    public class OrderBilledHandler : IHandleMessages<OrderBilled>
    {
        static ILog logger = LogManager.GetLogger<OrderBilledHandler>();

        public Task Handle(OrderBilled message, IMessageHandlerContext context)
        {
            logger.Info($"Received OrderBilled, OrderId = {message.OrderId} - shipment scheduled");
            return Task.CompletedTask;
        }
    }
}
