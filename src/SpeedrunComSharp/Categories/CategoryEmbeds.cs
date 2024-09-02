namespace SpeedrunComSharp;

public struct CategoryEmbeds
{
    private Embeds embeds;

    public bool EmbedGame { get { return embeds["game"]; } set { embeds["game"] = value; } }
    public bool EmbedVariables { get { return embeds["variables"]; } set { embeds["variables"] = value; } }

    /// <summary>
    /// Options for embedding resources in Category responses.
    /// </summary>
    /// <param name="embedGame">Dictates whether a Game object is included in the response.</param>
    /// <param name="embedVariables">Dictates whether a Collection of Variable objects is included in the response.</param>
    public CategoryEmbeds(
        bool embedGame = false,
        bool embedVariables = false)
    {
        embeds = new Embeds();
        EmbedGame = embedGame;
        EmbedVariables = embedVariables;
    }

    public override string ToString()
    {
        return embeds.ToString();
    }
}
