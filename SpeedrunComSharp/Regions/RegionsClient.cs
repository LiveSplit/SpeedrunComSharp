using System;
using System.Collections.Generic;
namespace SpeedrunComSharp
{
    public class RegionsClient
    {
        public const string Name = "regions";

        private SpeedrunComClient baseClient;

        public RegionsClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetRegionsUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
        }

        /// <summary>
        /// Fetch a Region object identified by its URI.
        /// </summary>
        /// <param name="siteUri">The site URI for the region.</param>
        /// <returns></returns>
        public Region GetRegionFromSiteUri(string siteUri)
        {
            var id = GetRegionIDFromSiteUri(siteUri);

            if (string.IsNullOrEmpty(id))
                return null;

            return GetRegion(id);
        }

        /// <summary>
        /// Fetch a Region ID identified by its URI.
        /// </summary>
        /// <param name="siteUri">The site URI for the region.</param>
        /// <returns></returns>
        public string GetRegionIDFromSiteUri(string siteUri)
        {
            var elementDescription = baseClient.GetElementDescriptionFromSiteUri(siteUri);

            if (elementDescription == null
                || elementDescription.Type != ElementType.Region)
                return null;

            return elementDescription.ID;
        }

        /// <summary>
        /// Fetch a Collection of Region objects.
        /// </summary>
        /// <param name="elementsPerPage">Optional. If included, will dictate the amount of elements included in each pagination.</param>
        /// <param name="orderBy">Optional. If omitted, regions will be in the same order as the API.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Fetch a Region object identified by its ID.
        /// </summary>
        /// <param name="regionId">The ID for the region.</param>
        /// <returns></returns>
        public Region GetRegion(string regionId)
        {
            var uri = GetRegionsUri(string.Format("/{0}", Uri.EscapeDataString(regionId)));
            var result = baseClient.DoRequest(uri);

            return Region.Parse(baseClient, result.data);
        }
    }
}
