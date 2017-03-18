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

namespace Rabbit.CustomRouting.GoldInsiderClientServices.EventHandlers
{
    public class OrderCancelledHandler : IHandleMessages<OrderCancelled>
    {
        static ILog logger = LogManager.GetLogger<OrderCancelledHandler>();

        public Task Handle(OrderCancelled message, IMessageHandlerContext context)
        {
            logger.Info($"Received Gold Insider OrderCancelled, OrderId = {message.OrderId} - doing some client services stuff for this very important client");
            return Task.CompletedTask;
        }
    }
}
