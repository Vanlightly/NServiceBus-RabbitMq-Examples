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
    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        static ILog logger = LogManager.GetLogger<OrderPlacedHandler>();

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            logger.Info($"Received Gold Insider OrderPlaced, OrderId = {message.OrderId} - doing some client services stuff for this very important client");
            return Task.CompletedTask;
        }
    }
}
