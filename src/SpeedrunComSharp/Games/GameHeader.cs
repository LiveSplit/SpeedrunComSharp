using System;

namespace SpeedrunComSharp;

/// <summary>
/// An optimized class for simple data for games.
/// </summary>
public class GameHeader : IElementWithID
{
    public string ID { get; private set; }
    public string Name { get; private set; }
    public string JapaneseName { get; private set; }
    public string TwitchName { get; private set; }
    public string Abbreviation { get; private set; }
    public Uri WebLink { get; private set; }

    private GameHeader() { }

    public static GameHeader Parse(SpeedrunComClient client, dynamic gameHeaderElement)
    {
        var gameHeader = new GameHeader
        {
            ID = gameHeaderElement.id as string,
            Name = gameHeaderElement.names.international as string,
            JapaneseName = gameHeaderElement.names.japanese as string,
            TwitchName = gameHeaderElement.names.twitch as string,
            WebLink = new Uri(gameHeaderElement.weblink as string),
            Abbreviation = gameHeaderElement.abbreviation as string
        };

        return gameHeader;
    }

    public override int GetHashCode()
    {
        return (ID ?? string.Empty).GetHashCode();
    }

    public override bool Equals(object obj)
    {
        var other = obj as GameHeader;

        if (other == null)
        {
            return false;
        }

        return ID == other.ID;
    }

    public override string ToString()
    {
        return Name;
    }
}
