using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public enum GamesOrdering : int
    {
        Name = 0,
        NameDescending,
        JapaneseName,
        JapaneseNameDescending,
        Abbreviation,
        AbbreviationDescending,
        YearOfRelease,
        YearOfReleaseDescending,
        CreationDate,
        CreationDateDescending
    }

    internal static class GamesOrderingHelpers
    {
        internal static IEnumerable<string> ToParameters(this GamesOrdering ordering)
        {
            var isDescending = ((int)ordering & 1) == 1;
            if (isDescending)
                ordering = (GamesOrdering)((int)ordering - 1);

            var str = "";

            switch (ordering)
            {
                case GamesOrdering.JapaneseName:
                    str = "name.jap"; break;
                case GamesOrdering.Abbreviation:
                    str = "abbreviation"; break;
                case GamesOrdering.YearOfRelease:
                    str = "released"; break;
                case GamesOrdering.CreationDate:
                    str = "created"; break;
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
