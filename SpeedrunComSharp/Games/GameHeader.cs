using System;

namespace SpeedrunComSharp
{
    public class GameHeader : IElementWithID
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string JapaneseName { get; private set; }
        public string Abbreviation { get; private set; }
        public Uri WebLink { get; private set; }

        private GameHeader() { }

        public static GameHeader Parse(SpeedrunComClient client, dynamic gameHeaderElement)
        {
            var gameHeader = new GameHeader();

            gameHeader.ID = gameHeaderElement.id as string;
            gameHeader.Name = gameHeaderElement.names.international as string;
            gameHeader.JapaneseName = gameHeaderElement.names.japanese as string;
            gameHeader.WebLink = new Uri(gameHeaderElement.weblink as string);
            gameHeader.Abbreviation = gameHeaderElement.abbreviation as string;

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
                return false;

            return ID == other.ID;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
