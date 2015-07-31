using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public enum RunsOrdering : int
    {
        Game = 0,
        GameDescending,
        Category,
        CategoryDescending,
        Level,
        LevelDescending,
        Platform,
        PlatformDescending,
        Region,
        RegionDescending,
        Emulated,
        EmulatedDescending,
        Date,
        DateDescending,
        DateSubmitted,
        DateSubmittedDescending,
        Status,
        StatusDescending,
        VerifyDate,
        VerifyDateDescending
    }

    internal static class RunsOrderingHelpers
    {
        internal static IEnumerable<string> ToParameters(this RunsOrdering ordering)
        {
            var isDescending = ((int)ordering & 1) == 1;
            if (isDescending)
                ordering = (RunsOrdering)((int)ordering - 1);

            var str = "";

            switch (ordering)
            {
                case RunsOrdering.Category:
                    str = "category"; break;
                case RunsOrdering.Level:
                    str = "level"; break;
                case RunsOrdering.Platform:
                    str = "platform"; break;
                case RunsOrdering.Region:
                    str = "region"; break;
                case RunsOrdering.Emulated:
                    str = "emulated"; break;
                case RunsOrdering.Date:
                    str = "date"; break;
                case RunsOrdering.DateSubmitted:
                    str = "submitted"; break;
                case RunsOrdering.Status:
                    str = "status"; break;
                case RunsOrdering.VerifyDate:
                    str = "verify-date"; break;
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
