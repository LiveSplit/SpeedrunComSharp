using System;
using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public class SeriesClient
    {
        public const string Name = "series";

        private SpeedrunComClient baseClient;

        public SeriesClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetSeriesUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
        }

        public Series GetSeriesFromSiteUri(string siteUri, SeriesEmbeds embeds = default(SeriesEmbeds))
        {
            var id = GetSeriesIDFromSiteUri(siteUri);

            if (string.IsNullOrEmpty(id))
                return null;

            return GetSingleSeries(id, embeds);
        }

        public string GetSeriesIDFromSiteUri(string siteUri)
        {
            var elementDescription = baseClient.GetElementDescriptionFromSiteUri(siteUri);

            if (elementDescription == null
                || elementDescription.Type != ElementType.Series)
                return null;

            return elementDescription.ID;
        }

        public IEnumerable<Series> GetMultipleSeries(
           string name = null, string abbreviation = null,
           string moderatorId = null, int? elementsPerPage = null,
           SeriesEmbeds embeds = default(SeriesEmbeds),
           SeriesOrdering orderBy = default(SeriesOrdering))
        {
            var parameters = new List<string>() { embeds.ToString() };

            parameters.AddRange(orderBy.ToParameters());

            if (!string.IsNullOrEmpty(name))
                parameters.Add(string.Format("name={0}", Uri.EscapeDataString(name)));

            if (!string.IsNullOrEmpty(abbreviation))
                parameters.Add(string.Format("abbreviation={0}", Uri.EscapeDataString(abbreviation)));

            if (!string.IsNullOrEmpty(moderatorId))
                parameters.Add(string.Format("moderator={0}", Uri.EscapeDataString(moderatorId)));

            if (elementsPerPage.HasValue)
                parameters.Add(string.Format("max={0}", elementsPerPage.Value));

            var uri = GetSeriesUri(parameters.ToParameters());
            return baseClient.DoPaginatedRequest(uri,
                x => Series.Parse(baseClient, x) as Series);
        }

        public Series GetSingleSeries(string seriesId, SeriesEmbeds embeds = default(SeriesEmbeds))
        {
            var parameters = new List<string>() { embeds.ToString() };

            var uri = GetSeriesUri(string.Format("/{0}{1}",
                Uri.EscapeDataString(seriesId),
                parameters.ToParameters()));

            var result = baseClient.DoRequest(uri);

            return Series.Parse(baseClient, result.data);
        }

        public IEnumerable<Game> GetGames(
            string seriesId,
            string name = null, int? yearOfRelease = null,
            string platformId = null, string regionId = null,
            string moderatorId = null, int? elementsPerPage = null,
            GameEmbeds embeds = default(GameEmbeds),
            GamesOrdering orderBy = default(GamesOrdering))
        {
            var parameters = new List<string>() { embeds.ToString() };

            parameters.AddRange(orderBy.ToParameters());

            if (!string.IsNullOrEmpty(name))
                parameters.Add(string.Format("name={0}", Uri.EscapeDataString(name)));

            if (yearOfRelease.HasValue)
                parameters.Add(string.Format("released={0}", yearOfRelease.Value));

            if (!string.IsNullOrEmpty(platformId))
                parameters.Add(string.Format("platform={0}", Uri.EscapeDataString(platformId)));

            if (!string.IsNullOrEmpty(regionId))
                parameters.Add(string.Format("region={0}", Uri.EscapeDataString(regionId)));

            if (!string.IsNullOrEmpty(moderatorId))
                parameters.Add(string.Format("moderator={0}", Uri.EscapeDataString(moderatorId)));

            if (elementsPerPage.HasValue)
                parameters.Add(string.Format("max={0}", elementsPerPage.Value));

            var uri = GetSeriesUri(string.Format("/{0}/games{1}",
                Uri.EscapeDataString(seriesId),
                parameters.ToParameters()));

            return baseClient.DoPaginatedRequest(uri,
                x => Game.Parse(baseClient, x) as Game);
        }
    }
}
