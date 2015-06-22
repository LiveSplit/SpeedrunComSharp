using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SpeedrunComSharp
{
    public class PlatformsClient
    {
        private SpeedrunComClient baseClient;

        public PlatformsClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetPlatformsUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("platforms{0}", subUri));
        }

        public IEnumerable<Platform> GetPlatforms(int? elementsPerPage = null)
        {
            var path = "";
            if (elementsPerPage.HasValue)
                path = "?max=" + elementsPerPage.Value;

            var uri = GetPlatformsUri(path);

            return baseClient.DoPaginatedRequest<Platform>(uri,
                x => Platform.Parse(baseClient, x) as Platform);
        }

        public Platform GetPlatform(string platformId)
        {
            var uri = GetPlatformsUri(string.Format("/{0}", HttpUtility.UrlPathEncode(platformId)));
            var result = baseClient.DoRequest(uri);

            return Platform.Parse(baseClient, result.data);
        }
    }
}
