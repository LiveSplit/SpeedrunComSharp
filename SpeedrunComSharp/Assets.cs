using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class Assets
    {
        public Uri Logo { get; private set; }
        public Uri CoverTiny { get; private set; }
        public Uri CoverSmall { get; private set; }
        public Uri CoverMedium { get; private set; }
        public Uri CoverLarge { get; private set; }
        public Uri Icon { get; private set; }
        public Uri TrophyFirstPlace { get; private set; }
        public Uri TrophySecondPlace { get; private set; }
        public Uri TrophyThirdPlace { get; private set; }
        public Uri TrophyFourthPlace { get; private set; }
        public Uri BackgroundImage { get; private set; }
        public Uri ForegroundImage { get; private set; }

        private Assets() { }

        private static Uri parseUri(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return null;
            else
                return new Uri(uri);
        }

        public static Assets Parse(SpeedrunComClient client, dynamic assetsElement)
        {
            var assets = new Assets();

            var properties = assetsElement.Properties as IDictionary<string, dynamic>;

            assets.Logo = parseUri(assetsElement.logo);
            assets.CoverTiny = parseUri(properties["cover-tiny"]);
            assets.CoverSmall = parseUri(properties["cover-small"]);
            assets.CoverMedium = parseUri(properties["cover-medium"]);
            assets.CoverLarge = parseUri(properties["cover-large"]);
            assets.Icon = parseUri(assetsElement.icon);
            assets.TrophyFirstPlace = parseUri(properties["trophy-1st"]);
            assets.TrophySecondPlace = parseUri(properties["trophy-2nd"]);
            assets.TrophyThirdPlace = parseUri(properties["trophy-3rd"]);
            assets.TrophyFourthPlace = parseUri(properties["trophy-4th"]);
            assets.BackgroundImage = parseUri(assetsElement.background);
            assets.ForegroundImage = parseUri(assetsElement.foreground);

            return assets;
        }
    }
}
