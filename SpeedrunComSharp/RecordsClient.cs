using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace SpeedrunComSharp
{
    public class RecordsClient
    {
        private SpeedrunComClient baseClient;

        public RecordsClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetRecordsUri(string subUri)
        {
            return SpeedrunComClient.GetSiteUri(string.Format("api_records.php{0}", subUri));
        }

        private string toParameter(TimingMethod method)
        {
            switch(method)
            {
                case TimingMethod.GameTime:
                    return "ingametime";
                case TimingMethod.RealTime:
                    return "timewithloads";
                case TimingMethod.RealTimeWithoutLoads:
                    return "timewithoutloads";
            }

            throw new ArgumentException("method");
        }

        public ReadOnlyCollection<Record> GetRecords(
            string gameName = null, string seriesName = null, 
            string userName = null, int? amount = null,
            TimingMethod? timingMethod = null)
        {
            var parameters = new List<string>() { "restids=yes" };

            if (!string.IsNullOrEmpty(gameName))
                parameters.Add(string.Format("game={0}", HttpUtility.UrlPathEncode(gameName)));
            if (!string.IsNullOrEmpty(seriesName))
                parameters.Add(string.Format("series={0}", HttpUtility.UrlPathEncode(seriesName)));
            if (!string.IsNullOrEmpty(userName))
                parameters.Add(string.Format("user={0}", HttpUtility.UrlPathEncode(userName)));
            if (amount.HasValue)
                parameters.Add(string.Format("amount={0}", amount.Value));
            if (timingMethod.HasValue)
            {
                var value = toParameter(timingMethod.Value);
                parameters.Add(string.Format("timing={0}", value));
            }

            var uri = GetRecordsUri(parameters.ToParameters());
            var result = JSON.FromUri(uri);

            var records = new List<Record>();

            var games = result.Properties as IDictionary<string, dynamic>;
            foreach (var game in games)
            {
                var parsedGameName = game.Key;
                var categories = game.Value.Properties as IDictionary<string, dynamic>;

                foreach (var category in categories)
                {
                    var parsedCategoryName = category.Key;
                    IDictionary<string, dynamic> categoryRecords;
                    if (amount.HasValue)
                    {
                        categoryRecords = category.Value as IDictionary<string, dynamic>;
                    }
                    else
                    {
                        categoryRecords = new Dictionary<string, dynamic>()
                        {
                            { "1", category.Value }
                        };
                    }

                    foreach (var categoryRecord in categoryRecords)
                    {
                        var place = Convert.ToInt32(categoryRecord.Key as string, CultureInfo.InvariantCulture);
                        var record = Record.Parse(baseClient, categoryRecord.Value, parsedGameName, parsedCategoryName, place) as Record;
                        records.Add(record);
                    }
                }
            }

            return records.AsReadOnly();
        }
    }
}
