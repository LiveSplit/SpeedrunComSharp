using System;
using System.Collections.Generic;
using System.Globalization;

namespace SpeedrunComSharp;

public class LeaderboardsClient
{
    public const string Name = "leaderboards";

    private readonly SpeedrunComClient baseClient;

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
        {
            parameters.Add(string.Format("top={0}", top.Value));
        }

        if (!string.IsNullOrEmpty(platformId))
        {
            parameters.Add(string.Format("platform={0}", Uri.EscapeDataString(platformId)));
        }

        if (!string.IsNullOrEmpty(regionId))
        {
            parameters.Add(string.Format("region={0}", Uri.EscapeDataString(regionId)));
        }

        if (emulatorsFilter != EmulatorsFilter.NotSet)
        {
            parameters.Add(string.Format("emulators={0}",
                emulatorsFilter == EmulatorsFilter.OnlyEmulators ? "true" : "false"));
        }

        if (filterOutRunsWithoutVideo)
        {
            parameters.Add("video-only=true");
        }

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
                if (variableValue != null)
                {
                    parameters.Add(string.Format("var-{0}={1}",
                    Uri.EscapeDataString(variableValue.VariableID),
                    Uri.EscapeDataString(variableValue.ID)));
                }
            }
        }

        var innerUri = GetLeaderboardsUri(string.Format("{0}{1}",
            uri,
            parameters.ToParameters()));

        var result = baseClient.DoRequest(innerUri);
        return Leaderboard.Parse(baseClient, result.data);
    }

    /// <summary>
    /// Fetch a Leaderboard object identified by the game ID and category ID.
    /// </summary>
    /// <param name="gameId">The ID for the game.</param>
    /// <param name="categoryId">The ID for the category.</param>
    /// <param name="top">Optional. If included, will dictate the amount of top runs included in the response.</param>
    /// <param name="platformId">Optional. If included, will filter runs by their platform.</param>
    /// <param name="regionId">Optional. If included, will filter runs by their region.</param>
    /// <param name="emulatorsFilter">Optional. If included, will filter runs by their use of emulator.</param>
    /// <param name="filterOutRunsWithoutVideo">Optional. If included, will dictate whether runs without video are included in the response.</param>
    /// <param name="orderBy">Optional. If omitted, runs will be in the same order as the API.</param>
    /// <param name="filterOutRunsAfter">Optional. If included, will filter out runs performed after the specified DateTime.</param>
    /// <param name="variableFilters">Optional. If included, will filter runs by the values present in specific variables.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Fetch a Leaderboard object identified by the game ID, level ID, and category ID.
    /// </summary>
    /// <param name="gameId">The ID for the game.</param>
    /// <param name="levelId">The ID for the level.</param>
    /// <param name="categoryId">The ID for the category.</param>
    /// <param name="top">Optional. If included, will dictate the amount of top runs included in the response.</param>
    /// <param name="platformId">Optional. If included, will filter runs by their platform.</param>
    /// <param name="regionId">Optional. If included, will filter runs by their region.</param>
    /// <param name="emulatorsFilter">Optional. If included, will filter runs by their use of emulator.</param>
    /// <param name="filterOutRunsWithoutVideo">Optional. If included, will dictate whether runs without video are included in the response.</param>
    /// <param name="orderBy">Optional. If omitted, runs will be in the same order as the API.</param>
    /// <param name="filterOutRunsAfter">Optional. If included, will filter out runs performed after the specified DateTime.</param>
    /// <param name="variableFilters">Optional. If included, will filter runs by the values present in specific variables.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <returns></returns>
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
