using System;

namespace SpeedrunComSharp
{
    public class APIException : ArgumentException
    {
        public APIException(string message)
            : base(message)
        { }
    }
}
