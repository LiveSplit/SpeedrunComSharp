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

        /// <summary>
        /// Options for embedding resources in Series responses.
        /// </summary>
        /// <param name="embedModerators">Dictates whether a Collection of User objects containing each moderator is included in the response.</param>
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
