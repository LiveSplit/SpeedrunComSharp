using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web;

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
            var elementDescription = SpeedrunComClient.GetElementDescriptionFromSiteUri(siteUri);

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
    }
}
