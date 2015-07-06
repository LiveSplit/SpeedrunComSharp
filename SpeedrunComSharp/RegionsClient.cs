using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SpeedrunComSharp
{
    public class RegionsClient
    {
        private SpeedrunComClient baseClient;

        public RegionsClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetRegionsUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("regions{0}", subUri));
        }

        public IEnumerable<Region> GetRegions(int? elementsPerPage = null,
            RegionsOrdering orderBy = default(RegionsOrdering))
        {
            var parameters = new List<string>();

            parameters.AddRange(orderBy.ToParameters());

            if (elementsPerPage.HasValue)
                parameters.Add(string.Format("max={0}", elementsPerPage.Value));

            var uri = GetRegionsUri(parameters.ToParameters());

            return baseClient.DoPaginatedRequest(uri,
                x => Region.Parse(baseClient, x) as Region);
        }

        public Region GetRegion(string regionId)
        {
            var uri = GetRegionsUri(string.Format("/{0}", Uri.EscapeDataString(regionId)));
            var result = baseClient.DoRequest(uri);

            return Region.Parse(baseClient, result.data);
        }
    }
}
