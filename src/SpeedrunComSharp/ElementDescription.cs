using System;

namespace SpeedrunComSharp;

public class ElementDescription
{
    public string ID { get; private set; }
    public ElementType Type { get; private set; }

    internal ElementDescription(string id, ElementType type)
    {
        ID = id;
        Type = type;
    }

    private static ElementType parseUriType(string type)
    {
        return type switch
        {
            CategoriesClient.Name => ElementType.Category,
            GamesClient.Name => ElementType.Game,
            GuestsClient.Name => ElementType.Guest,
            LevelsClient.Name => ElementType.Level,
            NotificationsClient.Name => ElementType.Notification,
            PlatformsClient.Name => ElementType.Platform,
            RegionsClient.Name => ElementType.Region,
            RunsClient.Name => ElementType.Run,
            SeriesClient.Name => ElementType.Series,
            UsersClient.Name => ElementType.User,
            VariablesClient.Name => ElementType.Variable,
            _ => throw new ArgumentException("type"),
        };
    }

    public static ElementDescription ParseUri(string uri)
    {
        string[] splits = uri.Split('/');

        if (splits.Length < 2)
        {
            return null;
        }

        string id = splits[splits.Length - 1];
        string uriTypeString = splits[splits.Length - 2];

        try
        {
            ElementType uriType = parseUriType(uriTypeString);
            return new ElementDescription(id, uriType);
        }
        catch
        {
            return null;
        }
    }
}
