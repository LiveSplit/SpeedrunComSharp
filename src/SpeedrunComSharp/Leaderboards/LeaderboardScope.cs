using System;

namespace SpeedrunComSharp
{
    public enum LeaderboardScope
    {
        All, FullGame, Levels
    }

    public static class LeaderboardScopeHelpers
    {
        public static string ToParameter(this LeaderboardScope scope)
        {
            switch (scope)
            {
                case LeaderboardScope.All:
                    return "all";
                case LeaderboardScope.FullGame:
                    return "full-game";
                case LeaderboardScope.Levels:
                    return "levels";
            }

            throw new ArgumentException("scope");
        }
    }
}
