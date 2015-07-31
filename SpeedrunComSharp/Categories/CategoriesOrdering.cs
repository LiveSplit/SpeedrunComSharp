using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public enum CategoriesOrdering : int
    {
        Position = 0,
        PositionDescending,
        Name,
        NameDescending,
        Miscellaneous,
        MiscellaneousDescending
    }

    internal static class CategoriesOrderingHelpers
    {
        internal static IEnumerable<string> ToParameters(this CategoriesOrdering ordering)
        {
            var isDescending = ((int)ordering & 1) == 1;
            if (isDescending)
                ordering = (CategoriesOrdering)((int)ordering - 1);

            var str = "";

            switch (ordering)
            {
                case CategoriesOrdering.Name:
                    str = "name"; break;
                case CategoriesOrdering.Miscellaneous:
                    str = "miscellaneous"; break;
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
