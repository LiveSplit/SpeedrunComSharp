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
        public const int AllRecords = 99999;
        
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
            var parameters = new List<string>();

            if (!string.IsNullOrEmpty(gameName))
                parameters.Add(string.Format("game={0}", Uri.EscapeDataString(gameName)));
            if (!string.IsNullOrEmpty(seriesName))
                parameters.Add(string.Format("series={0}", Uri.EscapeDataString(seriesName)));
            if (!string.IsNullOrEmpty(userName))
                parameters.Add(string.Format("user={0}", Uri.EscapeDataString(userName)));
            if (amount.HasValue)
                parameters.Add(string.Format("amount={0}", amount.Value));
            if (timingMethod.HasValue)
            {
                var value = toParameter(timingMethod.Value);
                parameters.Add(string.Format("timing={0}", value));
            }

            var uri = GetRecordsUri(parameters.ToParameters());
            
            var records = new List<Record>();
            dynamic result;
            
            try 
            {
                result = baseClient.DoRequest(uri);  
            }
            catch
            {
                return records.AsReadOnly();
            }

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
                        categoryRecords = category.Value.Properties as IDictionary<string, dynamic>;
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

        public Record GetWorldRecord(string gameName, string categoryName)
        {
            return GetRecords(gameName: gameName).FirstOrDefault(record => record.CategoryName == categoryName);
        }
    }
}
