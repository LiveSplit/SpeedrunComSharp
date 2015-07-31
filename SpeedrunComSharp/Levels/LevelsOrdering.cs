using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public enum LevelsOrdering : int
    {
        Position = 0,
        PositionDescending,
        Name,
        NameDescending
    }

    internal static class LevelsOrderingHelpers
    {
        internal static IEnumerable<string> ToParameters(this LevelsOrdering ordering)
        {
            var isDescending = ((int)ordering & 1) == 1;
            if (isDescending)
                ordering = (LevelsOrdering)((int)ordering - 1);

            var str = "";

            switch (ordering)
            {
                case LevelsOrdering.Name:
                    str = "name"; break;
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
