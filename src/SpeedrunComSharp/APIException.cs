using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpeedrunComSharp;

public class APIException : ArgumentException
{
    public ReadOnlyCollection<string> Errors { get; }

    public APIException(string message)
        : this(message, new List<string>().AsReadOnly())
    { }

    public APIException(string message, IEnumerable<string> errors)
        : base(message)
    {
        this.Errors = errors.ToList().AsReadOnly();
    }
}
