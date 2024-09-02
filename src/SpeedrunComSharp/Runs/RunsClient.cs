using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpeedrunComSharp;

public class RunsClient
{
    public const string Name = "runs";

    private readonly SpeedrunComClient baseClient;

    public RunsClient(SpeedrunComClient baseClient)
    {
        this.baseClient = baseClient;
    }

    public static Uri GetRunsUri(string subUri)
    {
        return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
    }

    /// <summary>
    /// Fetch a Run object identified by its URI.
    /// </summary>
    /// <param name="siteUri">The site URI for the run.</param>
    /// <param name="embeds">Optional. If included, will dictate the embedded resources included in the response.</param>
    /// <returns></returns>
    public Run GetRunFromSiteUri(string siteUri, RunEmbeds embeds = default)
    {
        var id = GetRunIDFromSiteUri(siteUri);

        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        return GetRun(id, embeds);
    }

    /// <summary>
    /// Fetch a Run ID identified by its URI.
    /// </summary>
    /// <param name="siteUri">The site URI for the run.</param>
    /// <returns></returns>
    public string GetRunIDFromSiteUri(string siteUri)
    {
        try
        {
            var match = Regex.Match(siteUri, "^(?:(?:https?://)?(?:www\\.)?speedrun\\.com/(?:\\w+/)?runs?/)?(\\w+)$");

            return match.Groups[1].Value;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Fetch a Collection of Run objects identified by the parameters provided.
    /// </summary>
    /// <param name="userId">Optional. If included, will filter runs by the user ID of the runner(s).</param>
    /// <param name="guestName">Optional. If included, will filter runs by the name of the guest runner(s).</param>
    /// <param name="examerUserId">Optional. If included, will filter runs by the user ID of the examiner.</param>
    /// <param name="gameId">Optional. If included, will filter runs by the ID of the corresponding game.</param>
    /// <param name="levelId">Optional. If included, will filter runs by the ID of the corresponding level.</param>
    /// <param name="categoryId">Optional. If included, will filter runs by the ID of the corresponding category</param>
    /// <param name="platformId">Optional. If included, will filter runs by their platform.</param>
    /// <param name="regionId">Optional. If included, will filter runs by their region.</param>
    /// <param name="onlyEmulatedRuns">Optional. If included, will filter runs by their use of emulator.</param>
    /// <param name="status">Optional. If included, will filter runs by their verification status.</param>
    /// <param name="elementsPerPage">Optional. If included, will dictate the amount of elements included in each pagination.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <param name="orderBy">Optional. If omitted, runs will be in the same order as the API.</param>
    /// <returns></returns>
    public IEnumerable<Run> GetRuns(
        string userId = null, string guestName = null,
        string examerUserId = null, string gameId = null,
        string levelId = null, string categoryId = null,
        string platformId = null, string regionId = null,
        bool onlyEmulatedRuns = false, RunStatusType? status = null,
        int? elementsPerPage = null,
        RunEmbeds embeds = default,
        RunsOrdering orderBy = default)
    {
        var parameters = new List<string>() { embeds.ToString() };

        if (!string.IsNullOrEmpty(userId))
        {
            parameters.Add(string.Format("user={0}", Uri.EscapeDataString(userId)));
        }

        if (!string.IsNullOrEmpty(guestName))
        {
            parameters.Add(string.Format("guest={0}", Uri.EscapeDataString(guestName)));
        }

        if (!string.IsNullOrEmpty(examerUserId))
        {
            parameters.Add(string.Format("examiner={0}", Uri.EscapeDataString(examerUserId)));
        }

        if (!string.IsNullOrEmpty(gameId))
        {
            parameters.Add(string.Format("game={0}", Uri.EscapeDataString(gameId)));
        }

        if (!string.IsNullOrEmpty(levelId))
        {
            parameters.Add(string.Format("level={0}", Uri.EscapeDataString(levelId)));
        }

        if (!string.IsNullOrEmpty(categoryId))
        {
            parameters.Add(string.Format("category={0}", Uri.EscapeDataString(categoryId)));
        }

        if (!string.IsNullOrEmpty(platformId))
        {
            parameters.Add(string.Format("platform={0}", Uri.EscapeDataString(platformId)));
        }

        if (!string.IsNullOrEmpty(regionId))
        {
            parameters.Add(string.Format("region={0}", Uri.EscapeDataString(regionId)));
        }

        if (onlyEmulatedRuns)
        {
            parameters.Add("emulated=yes");
        }

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
        {
            parameters.Add(string.Format("max={0}", elementsPerPage));
        }

        parameters.AddRange(orderBy.ToParameters());

        var uri = GetRunsUri(parameters.ToParameters());
        return baseClient.DoPaginatedRequest(uri,
            x => Run.Parse(baseClient, x) as Run);
    }

    /// <summary>
    /// Fetch a Run object identified by its ID.
    /// </summary>
    /// <param name="runId">The ID of the run.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <returns></returns>
    public Run GetRun(string runId,
        RunEmbeds embeds = default)
    {
        var parameters = new List<string>() { embeds.ToString() };

        var uri = GetRunsUri(string.Format("/{0}{1}",
            Uri.EscapeDataString(runId),
            parameters.ToParameters()));

        var result = baseClient.DoRequest(uri);

        return Run.Parse(baseClient, result.data);
    }

    /// <summary>
    /// Posts a Run object to Speedrun.com. Authentication is required for this action.
    /// </summary>
    /// <param name="categoryId">The ID of the category.</param>
    /// <param name="platformId">The ID of the platform.</param>
    /// <param name="levelId">Optional. If included, dictates the ID of the level.</param>
    /// <param name="date">Optional. If included, dictates the date of the run.</param>
    /// <param name="regionId">Optional. If included, dictates the ID of the region.</param>
    /// <param name="realTime">Optional. If included, dictates real time.</param>
    /// <param name="realTimeWithoutLoads">Optional. If included, dictates Real Time without loads.</param>
    /// <param name="gameTime">Optional. If included, dictates in game time.</param>
    /// <param name="emulated">Optional. If included, dictates whether the run was performed on emulator.</param>
    /// <param name="videoUri">Optional. If included, dictates the URI of the video.</param>
    /// <param name="comment">Optional. If included, dictates the comment of the run.</param>
    /// <param name="splitsIOUri">Optional. If included, dictates the URI of the Splits.IO page for the run.</param>
    /// <param name="variables">Optional. If included, dictates the variable values for the run.</param>
    /// <param name="verify">Optional. If included, dictates whether the run is verified automatically upon submitting.</param>
    /// <param name="simulateSubmitting">Optional. If included, dictates whether the run submission process is simulated.</param>
    /// <returns></returns>
    public Run Submit(string categoryId,
        string platformId,
        string levelId = null,
        DateTime? date = null,
        string regionId = null,
        TimeSpan? realTime = null,
        TimeSpan? realTimeWithoutLoads = null,
        TimeSpan? gameTime = null,
        bool? emulated = null,
        Uri videoUri = null,
        string comment = null,
        Uri splitsIOUri = null,
        IEnumerable<VariableValue> variables = null,
        bool? verify = null,
        bool simulateSubmitting = false)
    {
        var parameters = new List<string>();

        if (simulateSubmitting)
        {
            parameters.Add("dry=yes");
        }

        var uri = GetRunsUri(parameters.ToParameters());

        dynamic postBody = new DynamicJsonObject();
        dynamic runElement = new DynamicJsonObject();

        runElement.category = categoryId;
        runElement.platform = platformId;

        if (!string.IsNullOrEmpty(levelId))
        {
            runElement.level = levelId;
        }

        if (date.HasValue)
        {
            runElement.date = date.Value.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrEmpty(regionId))
        {
            runElement.region = regionId;
        }

        if (verify.HasValue)
        {
            runElement.verified = verify;
        }

        dynamic timesElement = new DynamicJsonObject();

        if (!realTime.HasValue
            && !realTimeWithoutLoads.HasValue
            && !gameTime.HasValue)
        {
            throw new APIException("You need to provide at least one time.");
        }

        if (realTime.HasValue)
        {
            timesElement.realtime = realTime.Value.TotalSeconds;
        }

        if (realTimeWithoutLoads.HasValue)
        {
            timesElement.realtime_noloads = realTimeWithoutLoads.Value.TotalSeconds;
        }

        if (gameTime.HasValue)
        {
            timesElement.ingame = gameTime.Value.TotalSeconds;
        }

        runElement.times = timesElement;

        if (emulated.HasValue)
        {
            runElement.emulated = emulated.Value;
        }

        if (videoUri != null)
        {
            runElement.video = videoUri.AbsoluteUri;
        }

        if (!string.IsNullOrEmpty(comment))
        {
            runElement.comment = comment;
        }

        if (splitsIOUri != null)
        {
            runElement.splitsio = splitsIOUri.PathAndQuery.Substring(splitsIOUri.PathAndQuery.LastIndexOf('/') + 1);
        }

        if (variables != null)
        {
            var variablesList = variables.ToList();

            if (variablesList.Any())
            {
                var variablesElement = new Dictionary<string, dynamic>();

                foreach (var variable in variablesList)
                {
                    var key = variable.VariableID;
                    dynamic value = new DynamicJsonObject();

                    if (variable.IsCustomValue)
                    {
                        value.type = "user-defined";
                        value.value = variable.Value;
                    }
                    else
                    {
                        value.type = "pre-defined";
                        value.value = variable.ID;
                    }

                    variablesElement.Add(key, value);
                }

                runElement.variables = variablesElement;
            }
        }

        postBody.run = runElement;

        var result = baseClient.DoPostRequest(uri, postBody.ToString());

        return Run.Parse(baseClient, result.data);
    }
}
