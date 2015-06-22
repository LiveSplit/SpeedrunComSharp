using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web;

namespace SpeedrunComSharp
{
    public class LevelsClient
    {
        private SpeedrunComClient baseClient;

        public LevelsClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetLevelsUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("levels{0}", subUri));
        }

        public Level GetLevel(string levelId, 
            LevelEmbeds embeds = default(LevelEmbeds))
        {
            var parameters = new List<string>() { embeds.ToString() };

            var uri = GetLevelsUri(string.Format("/{0}{1}",
                HttpUtility.UrlPathEncode(levelId), 
                parameters.ToParameters()));

            var result = baseClient.DoRequest(uri);

            return Level.Parse(baseClient, result.data);
        }

        public ReadOnlyCollection<Category> GetCategories(
            string levelId, bool miscellaneous = true,
            CategoryEmbeds embeds = default(CategoryEmbeds))
        {
            var parameters = new List<string>() { embeds.ToString() };

            if (!miscellaneous)
                parameters.Add("miscellaneous=no");

            var uri = GetLevelsUri(string.Format("/{0}/categories{1}", 
                HttpUtility.UrlPathEncode(levelId), 
                parameters.ToParameters()));

            return baseClient.DoDataCollectionRequest<Category>(uri,
                x => Category.Parse(baseClient, x));
        }

        public ReadOnlyCollection<Variable> GetVariables(string levelId)
        {
            var uri = GetLevelsUri(string.Format("/{0}/variables", HttpUtility.UrlPathEncode(levelId)));
            return baseClient.DoDataCollectionRequest<Variable>(uri,
                x => Variable.Parse(baseClient, x));
        }
    }
}
