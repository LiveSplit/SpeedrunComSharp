using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpeedrunComSharp
{
    public class APIException : ArgumentException
    {
        private readonly ReadOnlyCollection<string> errors;
        
        public ReadOnlyCollection<string> Errors { get { return errors; } }

        public APIException(string message)
            : this(message, new List<string>().AsReadOnly())
        { }

        public APIException(string message, IEnumerable<string> errors) 
            : base(message)
        {
            this.errors = errors.ToList().AsReadOnly();
        }
    }
}
