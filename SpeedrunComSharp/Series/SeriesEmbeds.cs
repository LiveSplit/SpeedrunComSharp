namespace SpeedrunComSharp
{
    public struct SeriesEmbeds
    {
        private Embeds embeds;

        public bool EmbedModerators
        {
            get { return embeds["moderators"]; }
            set { embeds["moderators"] = value; }
        }

        public SeriesEmbeds(
            bool embedModerators = false)
        {
            embeds = new Embeds();
            EmbedModerators = embedModerators;
        }

        public override string ToString()
        {
            return embeds.ToString();
        }
    }
}
