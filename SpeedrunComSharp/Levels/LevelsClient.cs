using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpeedrunComSharp
{
    public class LevelsClient
    {
        public const string Name = "levels";

        private SpeedrunComClient baseClient;

        public LevelsClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetLevelsUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
        }

        /// <summary>
        /// Fetch a Level object identified by its URI.
        /// </summary>
        /// <param name="siteUri">The site URI for the level.</param>
        /// <param name="embeds">Optional. If included, will dictate the embedded resources included in the response.</param>
        /// <returns></returns>
        public Level GetLevelFromSiteUri(string siteUri, LevelEmbeds embeds = default(LevelEmbeds))
        {
            var id = GetLevelIDFromSiteUri(siteUri);

            if (string.IsNullOrEmpty(id))
                return null;

            return GetLevel(id, embeds);
        }

        /// <summary>
        /// Fetch a Level ID identified by its URI.
        /// </summary>
        /// <param name="siteUri">The site URI for the level.</param>
        /// <returns></returns>
        public string GetLevelIDFromSiteUri(string siteUri)
        {
            var elementDescription = baseClient.GetElementDescriptionFromSiteUri(siteUri);

            if (elementDescription == null
                || elementDescription.Type != ElementType.Level)
                return null;

            return elementDescription.ID;
        }

        /// <summary>
        /// Fetch a Level object identified by its ID.
        /// </summary>
        /// <param name="levelId">The ID for the level.</param>
        /// <param name="embeds">Optional. If included, will dictate the embedded resources included in the response.</param>
        /// <returns></returns>
        public Level GetLevel(string levelId, 
            LevelEmbeds embeds = default(LevelEmbeds))
        {
            var parameters = new List<string>() { embeds.ToString() };

            var uri = GetLevelsUri(string.Format("/{0}{1}",
                Uri.EscapeDataString(levelId), 
                parameters.ToParameters()));

            var result = baseClient.DoRequest(uri);

            return Level.Parse(baseClient, result.data);
        }

        /// <summary>
        /// Fetch a Collection of Category objects from a level's ID.
        /// </summary>
        /// <param name="levelId">The ID for the level.</param>
        /// <param name="miscellaneous">Optional. If included, will dictate whether miscellaneous categories are included.</param>
        /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
        /// <param name="orderBy">Optional. If omitted, categories will be in the same order as the API.</param>
        /// <returns></returns>
        public ReadOnlyCollection<Category> GetCategories(
            string levelId, bool miscellaneous = true,
            CategoryEmbeds embeds = default(CategoryEmbeds),
            CategoriesOrdering orderBy = default(CategoriesOrdering))
        {
            var parameters = new List<string>() { embeds.ToString() };

            parameters.AddRange(orderBy.ToParameters());

            if (!miscellaneous)
                parameters.Add("miscellaneous=no");

            var uri = GetLevelsUri(string.Format("/{0}/categories{1}", 
                Uri.EscapeDataString(levelId), 
                parameters.ToParameters()));

            return baseClient.DoDataCollectionRequest<Category>(uri,
                x => Category.Parse(baseClient, x));
        }

        /// <summary>
        /// Fetch a Collection of Variable objects from a level's ID.
        /// </summary>
        /// <param name="levelId">The ID for the level.</param>
        /// <param name="orderBy">Optional. If omitted, variables will be in the same order as the API.</param>
        /// <returns></returns>
        public ReadOnlyCollection<Variable> GetVariables(string levelId,
            VariablesOrdering orderBy = default(VariablesOrdering))
        {
            var parameters = new List<string>(orderBy.ToParameters());

            var uri = GetLevelsUri(string.Format("/{0}/variables{1}", 
                Uri.EscapeDataString(levelId),
                parameters.ToParameters()));

            return baseClient.DoDataCollectionRequest<Variable>(uri,
                x => Variable.Parse(baseClient, x));
        }

        /// <summary>
        /// Fetch a Leaderboard object from a level's ID.
        /// </summary>
        /// <param name="levelId">The ID for the level.</param>
        /// <param name="top">Optional. If included, will dictate the amount of top runs included in the response.</param>
        /// <param name="skipEmptyLeaderboards">Optional. If included, will dictate whether or not empty leaderboards are included in the response.</param>
        /// <param name="elementsPerPage">Optional. If included, will dictate the amount of elements included in each pagination.</param>
        /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
        /// <returns></returns>
        public IEnumerable<Leaderboard> GetRecords(string levelId,
           int? top = null, bool skipEmptyLeaderboards = false,
           int? elementsPerPage = null,
           LeaderboardEmbeds embeds = default(LeaderboardEmbeds))
        {
            var parameters = new List<string>() { embeds.ToString() };

            if (top.HasValue)
                parameters.Add(string.Format("top={0}", top.Value));
            if (skipEmptyLeaderboards)
                parameters.Add("skip-empty=true");
            if (elementsPerPage.HasValue)
                parameters.Add(string.Format("max={0}", elementsPerPage.Value));

            var uri = GetLevelsUri(string.Format("/{0}/records{1}",
                Uri.EscapeDataString(levelId),
                parameters.ToParameters()));

            return baseClient.DoPaginatedRequest<Leaderboard>(uri,
                x => Leaderboard.Parse(baseClient, x));
        }
    }
}
