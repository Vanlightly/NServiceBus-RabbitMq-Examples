using Messages.Commands;
using NServiceBus;
using NServiceBus.Logging;
using Rabbit.Sales.CustomRecoveryPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Sales.CUstomRecoveryPolicy.CommandHandlers
{
    public class CancelOrderHandler : IHandleMessages<CancelOrder>
    {
        static ILog logger = LogManager.GetLogger<CancelOrderHandler>();

        public Task Handle(CancelOrder message, IMessageHandlerContext context)
        {
            logger.Info($"Received CancelOrder, OrderId = {message.OrderId}");

            // This is normally where some business logic would occur

            // SqlException is: Must declare the scalar variable - this will happen everytime
            // but the policy related to thse messages is a Transient By Default
            // and OrderNotFoundException are treated as persistent
            // so these messages will be retried up to the limit
            SqlExceptionCreator.NewSqlException(137); 

            var orderCancelled = new OrderCancelled
            {
                OrderId = message.OrderId
            };
            return context.Publish(orderCancelled);
        }
    }
}
