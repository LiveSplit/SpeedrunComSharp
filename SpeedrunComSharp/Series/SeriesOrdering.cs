using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public enum SeriesOrdering : int
    {
        Name = 0,
        NameDescending,
        JapaneseName,
        JapaneseNameDescending,
        Abbreviation,
        AbbreviationDescending,
        CreationDate,
        CreationDateDescending
    }

    internal static class SeriesOrderingHelpers
    {
        internal static IEnumerable<string> ToParameters(this SeriesOrdering ordering)
        {
            var isDescending = ((int)ordering & 1) == 1;
            if (isDescending)
                ordering = (SeriesOrdering)((int)ordering - 1);

            var str = "";

            switch (ordering)
            {
                case SeriesOrdering.JapaneseName:
                    str = "name.jap"; break;
                case SeriesOrdering.Abbreviation:
                    str = "abbreviation"; break;
                case SeriesOrdering.CreationDate:
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
