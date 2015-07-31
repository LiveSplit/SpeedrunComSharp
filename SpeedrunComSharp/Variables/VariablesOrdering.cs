using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public enum VariablesOrdering : int
    {
        Position = 0,
        PositionDescending,
        Name,
        NameDescending,
        Mandatory,
        MandatoryDescending,
        UserDefined,
        UserDefinedDescending
    }

    internal static class VariablesOrderHelpers
    {
        internal static IEnumerable<string> ToParameters(this VariablesOrdering ordering)
        {
            var isDescending = ((int)ordering & 1) == 1;
            if (isDescending)
                ordering = (VariablesOrdering)((int)ordering - 1);

            var str = "";

            switch (ordering)
            {
                case VariablesOrdering.Name:
                    str = "name"; break;
                case VariablesOrdering.Mandatory:
                    str = "mandatory"; break;
                case VariablesOrdering.UserDefined:
                    str = "user-defined"; break;
            }

            var list = new List<string>();

            if (!string.IsNullOrEmpty(str))
                list.Add(string.Format("orderby={0}", str));
            if (isDescending)
                list.Add("direction=desc");

            return list;
        }
    }
}
