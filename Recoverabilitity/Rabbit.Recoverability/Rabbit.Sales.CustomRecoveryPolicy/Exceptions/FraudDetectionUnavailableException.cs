using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Sales.CustomRecoveryPolicy.Exceptions
{
    class FraudDetectionUnavailableException : Exception
    {
        public FraudDetectionUnavailableException(string message)
            :base(message)
        {

        }
    }
}
