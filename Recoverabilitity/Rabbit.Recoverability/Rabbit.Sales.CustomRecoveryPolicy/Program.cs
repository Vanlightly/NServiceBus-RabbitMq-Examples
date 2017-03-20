using NServiceBus;
using NServiceBus.Persistence.Sql;
using NServiceBus.Transport;
using Rabbit.Sales.CustomRecoveryPolicy.Exceptions;
using Rabbit.Sales.CustomRecoveryPolicy.Policies;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Sales.CustomRecoveryPolicy
{
    class Program
    {
        static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            Console.Title = "Rabbit Sales";

            var endpointConfiguration = new EndpointConfiguration("Rabbit.Sales");

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=localhost");
            transport.UsePublisherConfirms(true);

            // disabled immediate retries and set delayed retries with a one second increment
            // run the view_delays.sql script to see the delays in action
            SetUpPolicies();
            var recoverability = endpointConfiguration.Recoverability();
            recoverability.CustomPolicy(OrderPlacedPolicy);
            recoverability.Delayed(
                customizations: delayed =>
                {
                    delayed.NumberOfRetries(20);
                    delayed.TimeIncrease(TimeSpan.FromSeconds(1));
                });

            recoverability.Immediate(
                customizations: imm =>
                {
                    imm.NumberOfRetries(0);
                });

            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            var connection = @"Data Source=(local);Initial Catalog=NsbRabbitMqRecoverability;Integrated Security=True";
            persistence.SqlVariant(SqlVariant.MsSqlServer);
            persistence.ConnectionBuilder(
                connectionBuilder: () =>
                {
                    return new SqlConnection(connection);
                });

            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.EnableInstallers();

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await endpointInstance.Stop().ConfigureAwait(false);
        }

        static void SetUpPolicies()
        {
            FailPolicy.CreatePolicyWithName("Messages.Commands.PlaceOrder")
                .PersistentByDefault()
                .ClassifyAsSemiTransient<FraudDetectionUnavailableException>()
                .AddTransientClassifier(new SqlErrorClassifier());

            FailPolicy.CreatePolicyWithName("Messages.Commands.CancelOrder")
                .TransientByDefault()
                .ClassifyAsPersistent<OrderNotFoundException>();
        }

        static RecoverabilityAction OrderPlacedPolicy(RecoverabilityConfig config, ErrorContext context)
        {
            var errorCategory = ErrorCategory.Unknown;

            if (context.Message.Headers.ContainsKey("NServiceBus.EnclosedMessageTypes"))
            {
                if(context.Message.Headers["NServiceBus.EnclosedMessageTypes"].StartsWith("Messages.Commands.PlaceOrder"))
                    errorCategory = FailPolicy.GetPolicy("Messages.Commands.PlaceOrder").GetCategory(context.Exception);
                else if (context.Message.Headers["NServiceBus.EnclosedMessageTypes"].StartsWith("Messages.Commands.CancelOrder"))
                    errorCategory = FailPolicy.GetPolicy("Messages.Commands.CancelOrder").GetCategory(context.Exception);
            }

            if (errorCategory == ErrorCategory.Persistent)
            {
                return RecoverabilityAction.MoveToError(config.Failed.ErrorQueue);
            }
            else if (errorCategory == ErrorCategory.SemiTransient)
            {
                return RecoverabilityAction.DelayedRetry(TimeSpan.FromSeconds(60));
            }

            // invocation of default recoverability policy
            return DefaultRecoverabilityPolicy.Invoke(config, context);
        }
    }
}
