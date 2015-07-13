using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class APIException : ArgumentException
    {
        public APIException(string message)
            : base(message)
        { }
    }
}
