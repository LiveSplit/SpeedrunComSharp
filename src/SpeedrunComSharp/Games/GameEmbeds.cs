namespace SpeedrunComSharp;

public struct GameEmbeds
{
    private Embeds embeds;

    public bool EmbedLevels
    {
        get { return embeds["levels"]; }
        set { embeds["levels"] = value; }
    }
    public bool EmbedCategories
    {
        get { return embeds["categories"]; }
        set { embeds["categories"] = value; }
    }
    public bool EmbedModerators
    {
        get { return embeds["moderators"]; }
        set { embeds["moderators"] = value; }
    }
    public bool EmbedPlatforms
    {
        get { return embeds["platforms"]; }
        set { embeds["platforms"] = value; }
    }
    public bool EmbedRegions
    {
        get { return embeds["regions"]; }
        set { embeds["regions"] = value; }
    }
    public bool EmbedVariables
    {
        get { return embeds["variables"]; }
        set { embeds["variables"] = value; }
    }

    /// <summary>
    /// Options for embedding resources in Game responses.
    /// </summary>
    /// <param name="embedLevels">Dictates whether a Collection of Level objects is included in the response.</param>
    /// <param name="embedCategories">Dictates whether a Collection of Category objects is included in the response.</param>
    /// <param name="embedModerators">Dictates whether a Collection of User objects containing each moderator is included in the response.</param>
    /// <param name="embedPlatforms">Dictates whether a Collection of Platform objects is included in the response.</param>
    /// <param name="embedRegions">Dictates whether a Collection of Region objects is included in the response.</param>
    /// <param name="embedVariables">Dictates whether a Collection of Variable objects is included in the response.</param>
    public GameEmbeds(
        bool embedLevels = false,
        bool embedCategories = false,
        bool embedModerators = false,
        bool embedPlatforms = false,
        bool embedRegions = false,
        bool embedVariables = false)
    {
        embeds = new Embeds();
        EmbedLevels = embedLevels;
        EmbedCategories = embedCategories;
        EmbedModerators = embedModerators;
        EmbedPlatforms = embedPlatforms;
        EmbedRegions = embedRegions;
        EmbedVariables = embedVariables;
    }

    public override string ToString()
    {
        return embeds.ToString();
    }
}
