using Messages.Commands;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Sales.DelayInfrastructure.CommandHandlers
{
    public class CancelOrderHandler
    {
        static ILog logger = LogManager.GetLogger<CancelOrderHandler>();

        public Task Handle(CancelOrder message, IMessageHandlerContext context)
        {
            logger.Info($"Received CancelOrder, OrderId = {message.OrderId}");

            // This is normally where some business logic would occur

            var orderCancelled = new OrderCancelled
            {
                OrderId = message.OrderId
            };
            return context.Publish(orderCancelled);
        }
    }
}
