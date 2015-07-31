using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public enum UsersOrdering : int
    {
        Name = 0,
        NameDescending,
        JapaneseName,
        JapaneseNameDescending,
        SignUpDate,
        SignUpDateDescending,
        Role,
        RoleDescending
    }

    internal static class UsersOrderingHelpers
    {
        internal static IEnumerable<string> ToParameters(this UsersOrdering ordering)
        {
            var isDescending = ((int)ordering & 1) == 1;
            if (isDescending)
                ordering = (UsersOrdering)((int)ordering - 1);

            var str = "";

            switch (ordering)
            {
                case UsersOrdering.JapaneseName:
                    str = "name.jap"; break;
                case UsersOrdering.SignUpDate:
                    str = "signup"; break;
                case UsersOrdering.Role:
                    str = "role"; break;
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
