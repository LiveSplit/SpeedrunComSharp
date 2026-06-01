using System.Collections.Generic;
using System.Linq;

namespace SpeedrunComSharp;

internal static class StringHelpers
{
    internal static string ToParameters(this string parameters)
    {
        if (string.IsNullOrEmpty(parameters))
        {
            return "";
        }
        else
        {
            return "?" + parameters;
        }
    }

    internal static string ToParameters(this IEnumerable<string> parameters)
    {
        var list = parameters.Where(x => !string.IsNullOrEmpty(x)).ToList();
        if (list.Count > 0)
        {
            return "?" + string.Join("&", list);
        }
        else
        {
            return "";
        }
    }
}
