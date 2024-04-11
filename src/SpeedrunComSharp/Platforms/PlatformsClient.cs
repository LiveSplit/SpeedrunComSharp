﻿using System;
using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public class PlatformsClient
    {
        public const string Name = "platforms";

        private SpeedrunComClient baseClient;

        public PlatformsClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetPlatformsUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
        }

        /// <summary>
        /// Fetch a Platform object identified by its URI.
        /// </summary>
        /// <param name="siteUri">The site URI for the platform.</param>
        /// <returns></returns>
        public Platform GetPlatformFromSiteUri(string siteUri)
        {
            var id = GetPlatformIDFromSiteUri(siteUri);

            if (string.IsNullOrEmpty(id))
                return null;

            return GetPlatform(id);
        }

        /// <summary>
        /// Fetch a Platform ID identified by its URI.
        /// </summary>
        /// <param name="siteUri">The site URI for the platform.</param>
        /// <returns></returns>
        public string GetPlatformIDFromSiteUri(string siteUri)
        {
            var elementDescription = baseClient.GetElementDescriptionFromSiteUri(siteUri);

            if (elementDescription == null
                || elementDescription.Type != ElementType.Platform)
                return null;

            return elementDescription.ID;
        }

        /// <summary>
        /// Fetch a Collection of Platform objects.
        /// </summary>
        /// <param name="elementsPerPage">Optional. If included, will dictate the amount of elements included in each pagination.</param>
        /// <param name="orderBy">Optional. If omitted, platforms will be in the same order as the API.</param>
        /// <returns></returns>
        public IEnumerable<Platform> GetPlatforms(int? elementsPerPage = null,
            PlatformsOrdering orderBy = default(PlatformsOrdering))
        {
            var parameters = new List<string>();

            parameters.AddRange(orderBy.ToParameters());

            if (elementsPerPage.HasValue)
                parameters.Add(string.Format("max={0}", elementsPerPage.Value));

            var uri = GetPlatformsUri(parameters.ToParameters());

            return baseClient.DoPaginatedRequest(uri,
                x => Platform.Parse(baseClient, x) as Platform);
        }

        /// <summary>
        /// Fetch a Platform object identified by its ID.
        /// </summary>
        /// <param name="platformId">The ID for the platform.</param>
        /// <returns></returns>
        public Platform GetPlatform(string platformId)
        {
            var uri = GetPlatformsUri(string.Format("/{0}", Uri.EscapeDataString(platformId)));
            var result = baseClient.DoRequest(uri);

            return Platform.Parse(baseClient, result.data);
        }
    }
}
