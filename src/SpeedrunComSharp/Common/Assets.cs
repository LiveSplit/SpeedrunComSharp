using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public class Assets
    {
        public ImageAsset Logo { get; private set; }
        public ImageAsset CoverTiny { get; private set; }
        public ImageAsset CoverSmall { get; private set; }
        public ImageAsset CoverMedium { get; private set; }
        public ImageAsset CoverLarge { get; private set; }
        public ImageAsset Icon { get; private set; }
        public ImageAsset TrophyFirstPlace { get; private set; }
        public ImageAsset TrophySecondPlace { get; private set; }
        public ImageAsset TrophyThirdPlace { get; private set; }
        public ImageAsset TrophyFourthPlace { get; private set; }
        public ImageAsset BackgroundImage { get; private set; }
        public ImageAsset ForegroundImage { get; private set; }

        private Assets() { }

        public static Assets Parse(SpeedrunComClient client, dynamic assetsElement)
        {
            var assets = new Assets();

            var properties = assetsElement.Properties as IDictionary<string, dynamic>;

            assets.Logo = ImageAsset.Parse(client, assetsElement.logo) as ImageAsset;
            assets.CoverTiny = ImageAsset.Parse(client, properties["cover-tiny"]) as ImageAsset;
            assets.CoverSmall = ImageAsset.Parse(client, properties["cover-small"]) as ImageAsset;
            assets.CoverMedium = ImageAsset.Parse(client, properties["cover-medium"]) as ImageAsset;
            assets.CoverLarge = ImageAsset.Parse(client, properties["cover-large"]) as ImageAsset;
            assets.Icon = ImageAsset.Parse(client, assetsElement.icon) as ImageAsset;
            assets.TrophyFirstPlace = ImageAsset.Parse(client, properties["trophy-1st"]) as ImageAsset;
            assets.TrophySecondPlace = ImageAsset.Parse(client, properties["trophy-2nd"]) as ImageAsset;
            assets.TrophyThirdPlace = ImageAsset.Parse(client, properties["trophy-3rd"]) as ImageAsset;
            assets.TrophyFourthPlace = ImageAsset.Parse(client, properties["trophy-4th"]) as ImageAsset;
            assets.BackgroundImage = ImageAsset.Parse(client, assetsElement.background) as ImageAsset;
            assets.ForegroundImage = ImageAsset.Parse(client, assetsElement.foreground) as ImageAsset;

            return assets;
        }
    }
}
