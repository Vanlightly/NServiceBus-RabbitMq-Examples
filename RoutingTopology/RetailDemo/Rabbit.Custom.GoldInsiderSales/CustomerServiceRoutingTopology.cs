using NServiceBus.Transport.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.Transport;
using RabbitMQ.Client;
using NServiceBus;
using System.Collections.Concurrent;

namespace Rabbit.CustomRouting.GoldInsiderCustomerService
{
    class CustomerServiceRoutingTopology : IRoutingTopology
    {
        readonly bool useDurableExchanges;

        public CustomerServiceRoutingTopology(bool useDurableExchanges)
        {
            this.useDurableExchanges = useDurableExchanges;
        }

        public void SetupSubscription(IModel channel, Type type, string subscriberName)
        {
            if (type == typeof(IEvent))
            {
                // Make handlers for IEvent handle all events whether they extend IEvent or not
                type = typeof(object);
            }

            SetupTypeSubscriptions(channel, type);
            channel.ExchangeBind(subscriberName, ExchangeName(type), string.Empty);
        }

        public void TeardownSubscription(IModel channel, Type type, string subscriberName)
        {
            try
            {
                channel.ExchangeUnbind(subscriberName, ExchangeName(type), string.Empty, null);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                // TODO: Any better way to make this idempotent?
            }
        }

        public void Publish(IModel channel, Type type, OutgoingMessage message, IBasicProperties properties)
        {
            SetupTypeSubscriptions(channel, type);
            channel.BasicPublish(ExchangeName(type), String.Empty, false, properties, message.Body);
        }

        public void Send(IModel channel, string address, OutgoingMessage message, IBasicProperties properties)
        {
            channel.BasicPublish(address, String.Empty, true, properties, message.Body);
        }

        public void RawSendInCaseOfFailure(IModel channel, string address, byte[] body, IBasicProperties properties)
        {
            channel.BasicPublish(address, String.Empty, true, properties, body);
        }
        
        public void BindToDelayInfrastructure(IModel channel, string address, string deliveryExchange, string routingKey)
        {
            channel.ExchangeBind(address, deliveryExchange, routingKey);
        }

        public void Initialize(IModel channel, string mainQueue)
        {
            if (mainQueue.Equals("Rabbit.CustomRouting.GoldInsiderCustomerService"))
            {
                CreateHeadersExchange(channel, mainQueue);

                var bindingArguments = new Dictionary<string, object>();
                bindingArguments.Add("x-match", "all");
                bindingArguments.Add("membership", "gold");
                bindingArguments.Add("insider", "1");

                channel.QueueBind(mainQueue, mainQueue, string.Empty, bindingArguments);
            }
            else
            {
                CreateExchange(channel, mainQueue);
                channel.QueueBind(mainQueue, mainQueue, string.Empty);
            }
        }

        static string ExchangeName(Type type) => type.Namespace + ":" + type.Name;

        void SetupTypeSubscriptions(IModel channel, Type type)
        {
            if (type == typeof(Object) || IsTypeTopologyKnownConfigured(type))
            {
                return;
            }

            var typeToProcess = type;
            CreateExchange(channel, ExchangeName(typeToProcess));
            var baseType = typeToProcess.BaseType;

            while (baseType != null)
            {
                CreateExchange(channel, ExchangeName(baseType));
                channel.ExchangeBind(ExchangeName(baseType), ExchangeName(typeToProcess), string.Empty);
                typeToProcess = baseType;
                baseType = typeToProcess.BaseType;
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                var exchangeName = ExchangeName(interfaceType);

                CreateExchange(channel, exchangeName);
                channel.ExchangeBind(exchangeName, ExchangeName(type), string.Empty);
            }

            MarkTypeConfigured(type);
        }

        void MarkTypeConfigured(Type eventType)
        {
            typeTopologyConfiguredSet[eventType] = null;
        }

        bool IsTypeTopologyKnownConfigured(Type eventType) => typeTopologyConfiguredSet.ContainsKey(eventType);

        void CreateExchange(IModel channel, string exchangeName)
        {
            try
            {
                channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, useDurableExchanges);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                // TODO: Any better way to make this idempotent?
            }
        }

        void CreateHeadersExchange(IModel channel, string exchangeName)
        {
            try
            {
                channel.ExchangeDeclare(exchangeName, ExchangeType.Headers, useDurableExchanges);
            }
            catch (Exception)
            {
                // TODO: Any better way to make this idempotent?
            }
        }

        readonly ConcurrentDictionary<Type, string> typeTopologyConfiguredSet = new ConcurrentDictionary<Type, string>();
    }
}

