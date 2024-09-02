namespace SpeedrunComSharp;

public struct LevelEmbeds
{
    private Embeds embeds;

    public bool EmbedCategories
    {
        get { return embeds["categories"]; }
        set { embeds["categories"] = value; }
    }
    public bool EmbedVariables
    {
        get { return embeds["variables"]; }
        set { embeds["variables"] = value; }
    }

    /// <summary>
    /// Options for embedding resources in Level responses.
    /// </summary>
    /// <param name="embedCategories">Dictates whether a Collection of Category objects is included in the response.</param>
    /// <param name="embedVariables">Dictates whether a Collection of Variable objects is included in the response.</param>
    public LevelEmbeds(
        bool embedCategories = false,
        bool embedVariables = false)
    {
        embeds = new Embeds();
        EmbedCategories = embedCategories;
        EmbedVariables = embedVariables;
    }

    public override string ToString()
    {
        return embeds.ToString();
    }
}
