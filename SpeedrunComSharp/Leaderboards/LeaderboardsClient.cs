using System;
using System.Collections.Generic;
using System.Globalization;

namespace SpeedrunComSharp
{
    public class LeaderboardsClient
    {
        public const string Name = "leaderboards";

        private SpeedrunComClient baseClient;

        public LeaderboardsClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetLeaderboardsUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
        }

        private Leaderboard getLeaderboard(
            string uri, int? top = null,
            string platformId = null, string regionId = null,
            EmulatorsFilter emulatorsFilter = EmulatorsFilter.NotSet, bool filterOutRunsWithoutVideo = false,
            TimingMethod? orderBy = null, DateTime? filterOutRunsAfter = null,
            IEnumerable<VariableValue> variableFilters = null,
            LeaderboardEmbeds embeds = default(LeaderboardEmbeds))
        {
            var parameters = new List<string>() { embeds.ToString() };

            if (top.HasValue)
                parameters.Add(string.Format("top={0}", top.Value));
            if (!string.IsNullOrEmpty(platformId))
                parameters.Add(string.Format("platform={0}", Uri.EscapeDataString(platformId)));
            if (!string.IsNullOrEmpty(regionId))
                parameters.Add(string.Format("region={0}", Uri.EscapeDataString(regionId)));
            if (emulatorsFilter != EmulatorsFilter.NotSet)
                parameters.Add(string.Format("emulators={0}",
                    emulatorsFilter == EmulatorsFilter.OnlyEmulators ? "true" : "false"));
            if (filterOutRunsWithoutVideo)
                parameters.Add("video-only=true");
            if (orderBy.HasValue)
            {
                var timing = orderBy.Value.ToAPIString();
                parameters.Add(string.Format("timing={0}", timing));
            }
            if (filterOutRunsAfter.HasValue)
            {
                var date = filterOutRunsAfter.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                parameters.Add(string.Format("date={0}", 
                    Uri.EscapeDataString(date)));
            }
            if (variableFilters != null)
            {
                foreach (var variableValue in variableFilters)
                {
                    parameters.Add(string.Format("var-{0}={1}",
                        Uri.EscapeDataString(variableValue.VariableID),
                        Uri.EscapeDataString(variableValue.ID)));
                }
            }

            var innerUri = GetLeaderboardsUri(string.Format("{0}{1}",
                uri,
                parameters.ToParameters()));

            var result = baseClient.DoRequest(innerUri);
            return Leaderboard.Parse(baseClient, result.data);
        }

        public Leaderboard GetLeaderboardForFullGameCategory(
            string gameId, string categoryId,
            int? top = null,
            string platformId = null, string regionId = null,
            EmulatorsFilter emulatorsFilter = EmulatorsFilter.NotSet, bool filterOutRunsWithoutVideo = false,
            TimingMethod? orderBy = null, DateTime? filterOutRunsAfter = null,
            IEnumerable<VariableValue> variableFilters = null,
            LeaderboardEmbeds embeds = default(LeaderboardEmbeds))
        {
            var uri = string.Format("/{0}/category/{1}",
                Uri.EscapeDataString(gameId),
                Uri.EscapeDataString(categoryId));

            return getLeaderboard(uri,
                top,
                platformId, regionId,
                emulatorsFilter, filterOutRunsWithoutVideo,
                orderBy, filterOutRunsAfter,
                variableFilters,
                embeds);
        }

        public Leaderboard GetLeaderboardForLevel(
            string gameId, string levelId, string categoryId,
            int? top = null,
            string platformId = null, string regionId = null,
            EmulatorsFilter emulatorsFilter = EmulatorsFilter.NotSet, bool filterOutRunsWithoutVideo = false,
            TimingMethod? orderBy = null, DateTime? filterOutRunsAfter = null,
            IEnumerable<VariableValue> variableFilters = null,
            LeaderboardEmbeds embeds = default(LeaderboardEmbeds))
        {
            var uri = string.Format("/{0}/level/{1}/{2}",
                Uri.EscapeDataString(gameId),
                Uri.EscapeDataString(levelId),
                Uri.EscapeDataString(categoryId));

            return getLeaderboard(uri,
                top,
                platformId, regionId,
                emulatorsFilter, filterOutRunsWithoutVideo,
                orderBy, filterOutRunsAfter,
                variableFilters,
                embeds);
        }
    }
}
