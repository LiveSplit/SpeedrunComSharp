using System;

namespace SpeedrunComSharp
{
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
            switch (type)
            {
                case CategoriesClient.Name:
                    return ElementType.Category;
                case GamesClient.Name:
                    return ElementType.Game;
                case GuestsClient.Name:
                    return ElementType.Guest;
                case LevelsClient.Name:
                    return ElementType.Level;
                case NotificationsClient.Name:
                    return ElementType.Notification;
                case PlatformsClient.Name:
                    return ElementType.Platform;
                case RegionsClient.Name:
                    return ElementType.Region;
                case RunsClient.Name:
                    return ElementType.Run;
                case SeriesClient.Name:
                    return ElementType.Series;
                case UsersClient.Name:
                    return ElementType.User;
                case VariablesClient.Name:
                    return ElementType.Variable;
            }
            throw new ArgumentException("type");
        }

        public static ElementDescription ParseUri(string uri)
        {
            var splits = uri.Split('/');

            if (splits.Length < 2)
                return null;

            var id = splits[splits.Length - 1];
            var uriTypeString = splits[splits.Length - 2];

            try
            {
                var uriType = parseUriType(uriTypeString);
                return new ElementDescription(id, uriType);
            }
            catch
            {
                return null;
            }
        }
    }
}
