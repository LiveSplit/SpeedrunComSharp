namespace SpeedrunComSharp
{
    public struct RunEmbeds
    {
        private Embeds embeds;

        public bool EmbedGame 
        { 
            get { return embeds["game"]; } 
            set { embeds["game"] = value; } 
        }

        public bool EmbedCategory
        {
            get { return embeds["category"]; }
            set { embeds["category"] = value; }
        }

        public bool EmbedLevel
        {
            get { return embeds["level"]; }
            set { embeds["level"] = value; }
        }

        public bool EmbedPlayers
        {
            get { return embeds["players"]; }
            set { embeds["players"] = value; }
        }

        public bool EmbedRegion
        {
            get { return embeds["region"]; }
            set { embeds["region"] = value; }
        }

        public bool EmbedPlatform
        {
            get { return embeds["platform"]; }
            set { embeds["platform"] = value; }
        }

        public RunEmbeds(
            bool embedGame = false,
            bool embedCategory = false,
            bool embedLevel = false,
            bool embedPlayers = false,
            bool embedRegion = false,
            bool embedPlatform = false)
        {
            embeds = new Embeds();
            EmbedGame = embedGame;
            EmbedCategory = embedCategory;
            EmbedLevel = embedLevel;
            EmbedPlayers = embedPlayers;
            EmbedRegion = embedRegion;
            EmbedPlatform = embedPlatform;
        }

        public override string ToString()
        {
            return embeds.ToString();
        }
    }
}
