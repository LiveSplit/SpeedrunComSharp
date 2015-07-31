using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public enum PlatformsOrdering : int
    {
        Name = 0,
        NameDescending,
        YearOfRelease,
        YearOfReleaseDescending
    }

    internal static class PlatformsOrderingHelpers
    {
        internal static IEnumerable<string> ToParameters(this PlatformsOrdering ordering)
        {
            var isDescending = ((int)ordering & 1) == 1;
            if (isDescending)
                ordering = (PlatformsOrdering)((int)ordering - 1);

            var str = "";

            switch (ordering)
            {
                case PlatformsOrdering.YearOfRelease:
                    str = "released"; break;
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
