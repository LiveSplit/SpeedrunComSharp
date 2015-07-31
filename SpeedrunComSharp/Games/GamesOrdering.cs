using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public enum GamesOrdering : int
    {
        Similarity = 0,
        SimilarityDescending,
        Name,
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
                case GamesOrdering.Name:
                    str = "name.int"; break;
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
