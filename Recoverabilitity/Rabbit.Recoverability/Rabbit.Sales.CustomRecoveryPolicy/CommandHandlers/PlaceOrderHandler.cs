using Messages.Commands;
using Messages.Events;
using NServiceBus;
using NServiceBus.Logging;
using Rabbit.Sales.CustomRecoveryPolicy;
using Rabbit.Sales.CustomRecoveryPolicy.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Sales.CommandHandlers
{
    public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        static ILog logger = LogManager.GetLogger<PlaceOrderHandler>();

        public Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            logger.Info($"Received PlaceOrder, OrderId = {message.OrderId}");

            LogTrace(message);

            // This is normally where some business logic would occur

            // 25% of messages always fail with a missing parameter error - persistent
            // all messages fail 10% of the time with a deadlock - transient
            // all messages fail 10% of the time with a FraudDetectionUnavailableException - transient
            var d = message.GetHashCode();
            if (d > (int.MaxValue / 4) * 3)
            {
                throw SqlExceptionCreator.NewSqlException(137); // Must declare the scalar variable - PERSISTENT
            }
            else if (new Random(Guid.NewGuid().GetHashCode()).NextDouble() > 0.90)
            {
                throw SqlExceptionCreator.NewSqlException(1205); // deadlock - TRANSIENT
            }
            else if (new Random(Guid.NewGuid().GetHashCode()).NextDouble() > 0.80)
            {
                throw new FraudDetectionUnavailableException("Fraud detection system is down for a short time"); // SEMI TRANSIENT
            }

            var orderPlaced = new OrderPlaced
            {
                OrderId = message.OrderId
            };
            return context.Publish(orderPlaced);
        }

        private void LogTrace(PlaceOrder message)
        {
            using (var sqlConn = new SqlConnection("Server=(local);Database=NsbRabbitMqRecoverability;Trusted_Connection=true;"))
            {
                sqlConn.Open();
                using (var command = sqlConn.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO [dbo].[RetriesTrace]
        ([OrderId]
        ,[AppEntryTime])
    VALUES
        (@OrderId
        ,@TimeNow)";

                    command.Parameters.Add("OrderId", SqlDbType.Int).Value = message.OrderId;
                    command.Parameters.Add("TimeNow", SqlDbType.DateTime).Value = DateTime.Now;

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
