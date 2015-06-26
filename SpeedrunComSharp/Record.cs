using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class Record
    {
        public string GameName { get; private set; }
        public string CategoryName { get; private set; }
        public string RunID { get; private set; }
        public int? Place { get; private set; }
        public string PlayerName { get { return PlayerNames.FirstOrDefault(); } }
        public ReadOnlyCollection<string> PlayerNames { get; private set; }
        public TimeSpan? RealTime { get; private set; }
        public TimeSpan? RealTimeWithoutLoads { get; private set; }
        public TimeSpan? GameTime { get; private set; }
        public DateTime? Date { get; private set; }
        public Uri Video { get; private set; }
        public Uri SplitsIOUri { get; private set; }
        public Uri WebLink { get; private set; }

        public bool SplitsAvailable { get { return SplitsIOUri != null; } }

        #region Links

        private Lazy<Run> run;
        internal Lazy<Game> game;
        internal Lazy<Category> category;

        public Run Run { get { return run.Value; } }
        public User Player { get { return Players.FirstOrDefault(); } }
        public IEnumerable<User> Players { get; private set; }
        public Game Game { get { return game.Value; } }
        public Category Category { get { return category.Value; } }

        #endregion

        private Record() { }

        public static Record Parse(SpeedrunComClient client, 
            dynamic recordElement, string gameName, 
            string categoryName, int place)
        {
            var record = new Record();

            var properties = recordElement.Properties as IDictionary<string, dynamic>;

            //Parse Attributes

            record.GameName = gameName;
            record.CategoryName = categoryName;
            record.RunID = recordElement.id as string;
            record.Place = place;

            if (properties.ContainsKey("place"))
                record.Place = Convert.ToInt32(recordElement.place as string, CultureInfo.InvariantCulture);

            var runners = new List<string>() { recordElement.player };
            string runnerKey;

            for (var i = 2; properties.ContainsKey(runnerKey = string.Format("player{0}", i)); ++i)
            {
                runners.Add(properties[runnerKey] as string);
            }

            record.PlayerNames = runners.AsReadOnly();

            if (recordElement.time != null)
            {
                record.RealTime = TimeSpan.FromSeconds(double.Parse(recordElement.time, CultureInfo.InvariantCulture));
            }

            if (properties.ContainsKey("timewithloads"))
            {
                //If the game supports Time without Loads, "time" actually returns Time without Loads
                record.RealTimeWithoutLoads = record.RealTime;

                //Real Time is then stored in timewithloads
                if (recordElement.timewithloads != null)
                    record.RealTime = TimeSpan.FromSeconds(double.Parse(recordElement.timewithloads, CultureInfo.InvariantCulture));
            }

            if (properties.ContainsKey("timeigt"))
            {
                if (recordElement.timeigt != null)
                    record.GameTime = TimeSpan.FromSeconds(double.Parse(recordElement.timeigt, CultureInfo.InvariantCulture));
            }

            if (!string.IsNullOrEmpty(recordElement.date))
                record.Date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    + TimeSpan.FromSeconds(
                        double.Parse(recordElement.date,
                        CultureInfo.InvariantCulture));

            var videoUri = recordElement.video as string;

            if (!string.IsNullOrEmpty(videoUri))
            {
                if (!videoUri.StartsWith("http"))
                    videoUri = "http://" + videoUri;

                if (Uri.IsWellFormedUriString(videoUri, UriKind.Absolute))
                    record.Video = new Uri(videoUri);
            }

            record.WebLink = new Uri(recordElement.links.web as string);

            string splitsIO = recordElement.splitsio as string;
            if (!string.IsNullOrEmpty(splitsIO))
                record.SplitsIOUri = new Uri(string.Format("https://splits.io/api/v3/runs/{0}", splitsIO));

            //Parse Links

            record.run = new Lazy<Run>(() => client.Runs.GetRun(record.RunID));
            record.Players = record.PlayerNames.Select(x => client.Users.GetUsers(name: x).FirstOrDefault()).Cache();
            record.game = new Lazy<Game>(() => client.Games.GetGames(name: record.GameName, elementsPerPage: 1).FirstOrDefault());
            record.category = new Lazy<Category>(() => record.Game.Categories.FirstOrDefault(x => x.Name == record.CategoryName));

            return record;
        }
    }
}
