using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages.Commands
{
    public class CancelOrder : ICommand
    {
        public string OrderId { get; set; }
        public string ClientId { get; set; }
    }
}
