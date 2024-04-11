using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpeedrunComSharp
{
    public class CategoriesClient
    {
        public const string Name = "categories";

        private SpeedrunComClient baseClient;

        public CategoriesClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetCategoriesUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
        }

        /// <summary>
        /// Fetch a Category object identified by its URI.
        /// </summary>
        /// <param name="siteUri">The site URI for the category.</param>
        /// <param name="embeds">Optional. If included, will dictate the embedded resources included in the response.</param>
        /// <returns></returns>
        public Category GetCategoryFromSiteUri(string siteUri, CategoryEmbeds embeds = default(CategoryEmbeds))
        {
            var id = GetCategoryIDFromSiteUri(siteUri);

            if (string.IsNullOrEmpty(id))
                return null;

            return GetCategory(id, embeds);
        }

        /// <summary>
        /// Fetch a Category ID identified by its URI.
        /// </summary>
        /// <param name="siteUri">The site URI for the category.</param>
        /// <returns></returns>
        public string GetCategoryIDFromSiteUri(string siteUri)
        {
            var elementDescription = baseClient.GetElementDescriptionFromSiteUri(siteUri);

            if (elementDescription == null
                || elementDescription.Type != ElementType.Category)
                return null;

            return elementDescription.ID;
        }

        /// <summary>
        /// Fetch a Category object identified by its ID.
        /// </summary>
        /// <param name="categoryId">The ID for the category.</param>
        /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
        /// <returns></returns>
        public Category GetCategory(string categoryId, CategoryEmbeds embeds = default(CategoryEmbeds))
        {
            var uri = GetCategoriesUri(string.Format("/{0}{1}", Uri.EscapeDataString(categoryId), embeds.ToString().ToParameters()));
            var result = baseClient.DoRequest(uri);

            return Category.Parse(baseClient, result.data);
        }

        /// <summary>
        /// Fetch a Collection of Variable objects from a category's ID.
        /// </summary>
        /// <param name="categoryId">The ID for the category.</param>
        /// <param name="orderBy">Optional. If omitted, variables will be in the same order as the API.</param>
        /// <returns></returns>
        public ReadOnlyCollection<Variable> GetVariables(string categoryId,
            VariablesOrdering orderBy = default(VariablesOrdering))
        {
            var parameters = new List<string>(orderBy.ToParameters());

            var uri = GetCategoriesUri(string.Format("/{0}/variables{1}", 
                Uri.EscapeDataString(categoryId), 
                parameters.ToParameters()));

            return baseClient.DoDataCollectionRequest<Variable>(uri,
                x => Variable.Parse(baseClient, x));
        }

        /// <summary>
        /// Fetch a Leaderboard object from a category's ID.
        /// </summary>
        /// <param name="categoryId">The ID for the category.</param>
        /// <param name="top">Optional. If included, will dictate the amount of top runs included in the response.</param>
        /// <param name="skipEmptyLeaderboards">Optional. If included, will dictate whether or not empty leaderboards are included in the response.</param>
        /// <param name="elementsPerPage">Optional. If included, will dictate the amount of elements included in each pagination.</param>
        /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
        /// <returns></returns>
        public IEnumerable<Leaderboard> GetRecords(string categoryId,
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

            var uri = GetCategoriesUri(string.Format("/{0}/records{1}",
                Uri.EscapeDataString(categoryId),
                parameters.ToParameters()));

            return baseClient.DoPaginatedRequest<Leaderboard>(uri,
                x => Leaderboard.Parse(baseClient, x));
        }
    }
}
