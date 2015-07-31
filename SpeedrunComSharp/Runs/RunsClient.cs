using System;
using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public class RunsClient
    {
        public const string Name = "runs";

        private SpeedrunComClient baseClient;

        public RunsClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetRunsUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
        }

        public Run GetRunFromSiteUri(string siteUri, RunEmbeds embeds = default(RunEmbeds))
        {
            var id = GetRunIDFromSiteUri(siteUri);

            if (string.IsNullOrEmpty(id))
                return null;

            return GetRun(id, embeds);
        }

        public string GetRunIDFromSiteUri(string siteUri)
        {
            var elementDescription = SpeedrunComClient.GetElementDescriptionFromSiteUri(siteUri);

            if (elementDescription == null 
                || elementDescription.Type != ElementType.Run)
                return null;

            return elementDescription.ID;
        }

        public IEnumerable<Run> GetRuns(
            string userId = null, string guestName = null,
            string examerUserId = null, string gameId = null,
            string levelId = null, string categoryId = null,
            string platformId = null, string regionId = null,
            bool onlyEmulatedRuns = false, RunStatusType? status = null,
            int? elementsPerPage = null,
            RunEmbeds embeds = default(RunEmbeds),
            RunsOrdering orderBy = default(RunsOrdering))
        {
            var parameters = new List<string>() { embeds.ToString() };

            if (!string.IsNullOrEmpty(userId))
                parameters.Add(string.Format("user={0}", Uri.EscapeDataString(userId)));
            if (!string.IsNullOrEmpty(guestName))
                parameters.Add(string.Format("guest={0}", Uri.EscapeDataString(guestName)));
            if (!string.IsNullOrEmpty(examerUserId))
                parameters.Add(string.Format("examiner={0}", Uri.EscapeDataString(examerUserId)));
            if (!string.IsNullOrEmpty(gameId))
                parameters.Add(string.Format("game={0}", Uri.EscapeDataString(gameId)));
            if (!string.IsNullOrEmpty(levelId))
                parameters.Add(string.Format("level={0}", Uri.EscapeDataString(levelId)));
            if (!string.IsNullOrEmpty(categoryId))
                parameters.Add(string.Format("category={0}", Uri.EscapeDataString(categoryId)));
            if (!string.IsNullOrEmpty(platformId))
                parameters.Add(string.Format("platform={0}", Uri.EscapeDataString(platformId)));
            if (!string.IsNullOrEmpty(regionId))
                parameters.Add(string.Format("region={0}", Uri.EscapeDataString(regionId)));
            if (onlyEmulatedRuns)
                parameters.Add("emulated=yes");
            if (status.HasValue)
            {
                switch (status.Value)
                {
                    case RunStatusType.New:
                        parameters.Add("status=new"); break;
                    case RunStatusType.Rejected:
                        parameters.Add("status=rejected"); break;
                    case RunStatusType.Verified:
                        parameters.Add("status=verified"); break;
                }
            }
            if (elementsPerPage.HasValue)
                parameters.Add(string.Format("max={0}", elementsPerPage));

            parameters.AddRange(orderBy.ToParameters());

            var uri = GetRunsUri(parameters.ToParameters());
            return baseClient.DoPaginatedRequest(uri,
                x => Run.Parse(baseClient, x) as Run);
        }

        public Run GetRun(string runId,
            RunEmbeds embeds = default(RunEmbeds))
        {
            var parameters = new List<string>() { embeds.ToString() };

            var uri = GetRunsUri(string.Format("/{0}{1}",
                Uri.EscapeDataString(runId),
                parameters.ToParameters()));

            var result = baseClient.DoRequest(uri);

            return Run.Parse(baseClient, result.data);
        }
    }
}
