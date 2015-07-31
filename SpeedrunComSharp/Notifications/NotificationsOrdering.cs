using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public enum NotificationsOrdering : int
    {
        NewestToOldest = 0,
        OldestToNewest
    }

    internal static class NotificationsOrderingHelpers
    {
        internal static IEnumerable<string> ToParameters(this NotificationsOrdering ordering)
        {
            var list = new List<string>();

            if (ordering == NotificationsOrdering.OldestToNewest)
                list.Add("direction=asc");

            return list;
        }
    }
}
