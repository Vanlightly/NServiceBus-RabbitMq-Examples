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
    public class OrderCancelledHandler : IHandleMessages<OrderCancelled>
    {
        static ILog logger = LogManager.GetLogger<OrderCancelledHandler>();

        public Task Handle(OrderCancelled message, IMessageHandlerContext context)
        {
            logger.Info($"Received OrderCancelled, OrderId = {message.OrderId} - reembursing credit card");
            return Task.CompletedTask;
        }
    }
}
