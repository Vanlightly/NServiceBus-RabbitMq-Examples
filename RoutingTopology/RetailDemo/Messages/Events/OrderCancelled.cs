using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages.Events
{
    public class OrderCancelled : IEvent
    {
        public string OrderId { get; set; }
        public string ClientId { get; set; }
    }
}
