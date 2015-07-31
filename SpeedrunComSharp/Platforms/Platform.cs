using System.Collections;
using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public class Platform : IElementWithID
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public int YearOfRelease { get; private set; }

        #region Links

        public IEnumerable<Game> Games { get; private set; }
        public IEnumerable<Run> Runs { get; private set; }

        #endregion

        private Platform() { }

        public static Platform Parse(SpeedrunComClient client, dynamic platformElement)
        {
            if (platformElement is ArrayList)
                return null;

            var platform = new Platform();

            //Parse Attributes

            platform.ID = platformElement.id as string;
            platform.Name = platformElement.name as string;
            platform.YearOfRelease = (int)platformElement.released;

            //Parse Links

            platform.Games = client.Games.GetGames(platformId: platform.ID);
            platform.Runs = client.Runs.GetRuns(platformId: platform.ID);

            return platform;
        }

        public override int GetHashCode()
        {
            return (ID ?? string.Empty).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Platform;

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
