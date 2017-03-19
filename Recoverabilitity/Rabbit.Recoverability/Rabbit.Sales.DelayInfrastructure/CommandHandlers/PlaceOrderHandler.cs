using Messages.Commands;
using Messages.Events;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Sales.DelayInfrastructure.CommandHandlers
{
    public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        static ILog logger = LogManager.GetLogger<PlaceOrderHandler>();

        public Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            logger.Info($"Received PlaceOrder, OrderId = {message.OrderId}");

            LogTrace(message);

            // This is normally where some business logic would occur
            throw new Exception("An exception occurred in the handler.");

            var orderPlaced = new OrderPlaced
            {
                OrderId = message.OrderId
            };
            return context.Publish(orderPlaced);
        }

        private void LogTrace(PlaceOrder message)
        {
            try
            {
                using (var sqlConn = new SqlConnection("Server=(local);Database=NsbRabbitMqRecoverability;Trusted_Connection=true;"))
                {
                    sqlConn.Open();
                    var command = sqlConn.CreateCommand();
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
            catch(Exception ex)
            {

            }
        }
    }
}
