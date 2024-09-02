namespace SpeedrunComSharp;

public struct RunEmbeds
{
    private Embeds embeds;

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

    public bool EmbedRegion
    {
        get { return embeds["region"]; }
        set { embeds["region"] = value; }
    }

    public bool EmbedPlatform
    {
        get { return embeds["platform"]; }
        set { embeds["platform"] = value; }
    }

    /// <summary>
    /// Options for embedding resources in Run responses.
    /// </summary>
    /// <param name="embedGame">Dictates whether a Game object is included in the response.</param>
    /// <param name="embedCategory">Dictates whether a Category object is included in the response.</param>
    /// <param name="embedLevel">Dictates whether a Level object is included in the response.</param>
    /// <param name="embedPlayers">Dictates whether a Collection of Runner objects containing each runner is included in the response.</param>
    /// <param name="embedRegion">Dictates whether a Region object is included in the response.</param>
    /// <param name="embedPlatform">Dictates whether a Platform object is included in the response.</param>
    public RunEmbeds(
        bool embedGame = false,
        bool embedCategory = false,
        bool embedLevel = false,
        bool embedPlayers = false,
        bool embedRegion = false,
        bool embedPlatform = false)
    {
        embeds = new Embeds();
        EmbedGame = embedGame;
        EmbedCategory = embedCategory;
        EmbedLevel = embedLevel;
        EmbedPlayers = embedPlayers;
        EmbedRegion = embedRegion;
        EmbedPlatform = embedPlatform;
    }

    public override string ToString()
    {
        return embeds.ToString();
    }
}
