using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Exceptions
{
    public class EndQueueException : Exception
    {
        public EndQueueException() : base("Queue is empty")
        {

        }
    }
}
