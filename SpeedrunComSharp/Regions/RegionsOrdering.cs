using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public enum RegionsOrdering : int
    {
        Name = 0,
        NameDescending,
    }

    internal static class RegionsOrderingHelpers
    {
        internal static IEnumerable<string> ToParameters(this RegionsOrdering ordering)
        {
            var isDescending = ((int)ordering & 1) == 1;
            if (isDescending)
                ordering = (RegionsOrdering)((int)ordering - 1);

            var str = "";

            /*switch (ordering)
            {
            }*/

            var list = new List<string>();

            if (!string.IsNullOrEmpty(str))
                list.Add(string.Format("orderby={0}", str));
            if (isDescending)
                list.Add("direction=desc");

            return list;
        }
    }
}
