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

        public Category GetCategoryFromSiteUri(string siteUri, CategoryEmbeds embeds = default(CategoryEmbeds))
        {
            var id = GetCategoryIDFromSiteUri(siteUri);

            if (string.IsNullOrEmpty(id))
                return null;

            return GetCategory(id, embeds);
        }

        public string GetCategoryIDFromSiteUri(string siteUri)
        {
            var elementDescription = baseClient.GetElementDescriptionFromSiteUri(siteUri);

            if (elementDescription == null
                || elementDescription.Type != ElementType.Category)
                return null;

            return elementDescription.ID;
        }

        public Category GetCategory(string categoryId, CategoryEmbeds embeds = default(CategoryEmbeds))
        {
            var uri = GetCategoriesUri(string.Format("/{0}{1}", Uri.EscapeDataString(categoryId), embeds.ToString().ToParameters()));
            var result = baseClient.DoRequest(uri);

            return Category.Parse(baseClient, result.data);
        }

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
