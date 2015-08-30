using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
            var elementDescription = baseClient.GetElementDescriptionFromSiteUri(siteUri);

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
                parameters.Add("dry=yes");

            var uri = GetRunsUri(parameters.ToParameters());

            dynamic postBody = new DynamicJsonObject();
            dynamic runElement = new DynamicJsonObject();

            runElement.category = categoryId;
            runElement.platform = platformId;

            if (!string.IsNullOrEmpty(levelId))
                runElement.level = levelId;

            if (date.HasValue)
                runElement.date = date.Value.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            if (!string.IsNullOrEmpty(regionId))
                runElement.region = regionId;

            if (verify.HasValue)
                runElement.verified = verify;

            dynamic timesElement = new DynamicJsonObject();

            if (!realTime.HasValue
                && !realTimeWithoutLoads.HasValue
                && !gameTime.HasValue)
            {
                throw new APIException("You need to provide at least one time.");
            }

            if (realTime.HasValue)
                timesElement.realtime = realTime.Value.TotalSeconds;

            if (realTimeWithoutLoads.HasValue)
                timesElement.realtime_noloads = realTimeWithoutLoads.Value.TotalSeconds;

            if (gameTime.HasValue)
                timesElement.ingame = gameTime.Value.TotalSeconds;

            runElement.times = timesElement;

            if (emulated.HasValue)
                runElement.emulated = emulated.Value;

            if (videoUri != null)
                runElement.video = videoUri.AbsoluteUri;

            if (!string.IsNullOrEmpty(comment))
                runElement.comment = comment;

            if (splitsIOUri != null)
                runElement.splitsio = splitsIOUri.PathAndQuery.Substring(splitsIOUri.PathAndQuery.LastIndexOf('/') + 1);

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
}
