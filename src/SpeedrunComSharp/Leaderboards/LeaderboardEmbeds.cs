namespace SpeedrunComSharp;

public struct LeaderboardEmbeds
{
    private Embeds embeds;
    private readonly bool isConstructed;

    public bool EmbedGame
    {
        get { return embeds["game"]; }
        set { embeds["game"] = value; }
    }

    public bool EmbedCategory
    {
        get { return embeds["category"]; }
        set { embeds["category"] = value; }
    }

    public bool EmbedLevel
    {
        get { return embeds["level"]; }
        set { embeds["level"] = value; }
    }

    public bool EmbedPlayers
    {
        get { return embeds["players"]; }
        set { embeds["players"] = value; }
    }

    public bool EmbedRegions
    {
        get { return embeds["regions"]; }
        set { embeds["regions"] = value; }
    }

    public bool EmbedPlatforms
    {
        get { return embeds["platforms"]; }
        set { embeds["platforms"] = value; }
    }

    public bool EmbedVariables
    {
        get { return embeds["variables"]; }
        set { embeds["variables"] = value; }
    }

    /// <summary>
    /// Options for embedding resources in Leaderboard responses.
    /// </summary>
    /// <param name="embedGame">Dictates whether a Game object is included in the response.</param>
    /// <param name="embedCategory">Dictates whether a Category object is included in the response.</param>
    /// <param name="embedLevel">Dictates whether a Level object is included in the response.</param>
    /// <param name="embedPlayers">Dictates whether a Collection of Player objects is included in the response.</param>
    /// <param name="embedRegions">Dictates whether a Collection of Region objects is included in the response.</param>
    /// <param name="embedPlatforms">Dictates whether a Collection of Platform objects is included in the response.</param>
    /// <param name="embedVariables">Dictates whether a Collection of Variable objects is included in the response.</param>
    public LeaderboardEmbeds(
        bool embedGame = false,
        bool embedCategory = false,
        bool embedLevel = false,
        bool embedPlayers = true,
        bool embedRegions = false,
        bool embedPlatforms = false,
        bool embedVariables = false)
    {
        isConstructed = true;

        embeds = new Embeds();
        EmbedGame = embedGame;
        EmbedCategory = embedCategory;
        EmbedLevel = embedLevel;
        EmbedPlayers = embedPlayers;
        EmbedRegions = embedRegions;
        EmbedPlatforms = embedPlatforms;
        EmbedVariables = embedVariables;
    }

    public override string ToString()
    {
        if (!isConstructed)
        {
            EmbedPlayers = true;
        }

        return embeds.ToString();
    }
}
