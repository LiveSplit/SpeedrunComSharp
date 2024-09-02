using System;

namespace SpeedrunComSharp;

public enum LeaderboardScope
{
    All, FullGame, Levels
}

public static class LeaderboardScopeHelpers
{
    public static string ToParameter(this LeaderboardScope scope)
    {
        return scope switch
        {
            LeaderboardScope.All => "all",
            LeaderboardScope.FullGame => "full-game",
            LeaderboardScope.Levels => "levels",
            _ => throw new ArgumentException("scope"),
        };
    }
}
