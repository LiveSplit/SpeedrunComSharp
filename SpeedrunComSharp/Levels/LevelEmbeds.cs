namespace SpeedrunComSharp
{
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
}
