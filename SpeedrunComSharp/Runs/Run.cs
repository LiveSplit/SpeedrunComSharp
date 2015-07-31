using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace SpeedrunComSharp
{
    public class Run : IElementWithID
    {
        public string ID { get; private set; }
        public Uri WebLink { get; private set; }
        public string GameID { get; private set; }
        public string LevelID { get; private set; }
        public string CategoryID { get; private set; }
        public RunVideos Videos { get; private set; }
        public string Comment { get; private set; }
        public RunStatus Status { get; private set; }
        public Player Player { get { return Players.FirstOrDefault(); } }
        public ReadOnlyCollection<Player> Players { get; internal set; }
        public DateTime? Date { get; private set; }
        public DateTime? DateSubmitted { get; private set; }
        public RunTimes Times { get; private set; }
        public RunSystem System { get; private set; }
        public Uri SplitsUri { get; private set; }
        public bool SplitsAvailable { get { return SplitsUri != null; } }
        public ReadOnlyCollection<VariableValue> VariableValues { get; private set; }

        #region Links

        internal Lazy<Game> game;
        internal Lazy<Category> category;
        private Lazy<Level> level;
        private Lazy<User> examiner;

        public Game Game { get { return game.Value; } }
        public Category Category { get { return category.Value; } }
        public Level Level { get { return level.Value; } }
        public Platform Platform { get { return System.Platform; } }
        public Region Region { get { return System.Region; } }
        public User Examiner { get { return examiner.Value; } }

        #endregion

        protected Run() { }

        internal static void Parse(Run run, SpeedrunComClient client, dynamic runElement)
        {
            //Parse Attributes

            run.ID = runElement.id as string;
            run.WebLink = new Uri(runElement.weblink as string);
            run.Videos = RunVideos.Parse(client, runElement.videos) as RunVideos;
            run.Comment = runElement.comment as string;
            run.Status = RunStatus.Parse(client, runElement.status) as RunStatus;

            Func<dynamic, Player> parsePlayer = x => Player.Parse(client, x) as Player;

            if (runElement.players is IEnumerable<dynamic>)
            {
                run.Players = client.ParseCollection(runElement.players, parsePlayer);
            }
            else
            {
                run.Players = client.ParseCollection(runElement.players.data, parsePlayer);
            }

            var runDate = runElement.date;
            if (!string.IsNullOrEmpty(runDate))
                run.Date = DateTime.Parse(runDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

            var dateSubmitted = runElement.submitted;
            if (!string.IsNullOrEmpty(dateSubmitted))
                run.DateSubmitted = DateTime.Parse(dateSubmitted, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

            run.Times = RunTimes.Parse(client, runElement.times) as RunTimes;
            run.System = RunSystem.Parse(client, runElement.system) as RunSystem;

            var splits = runElement.splits;
            if (splits != null)
            {
                run.SplitsUri = new Uri(splits.uri as string);
            }

            if (runElement.values is DynamicJsonObject)
            {
                var valueProperties = runElement.values.Properties as IDictionary<string, dynamic>;
                run.VariableValues = valueProperties.Select(x => VariableValue.ParseValueDescriptor(client, x) as VariableValue).ToList().AsReadOnly();
            }
            else
            {
                run.VariableValues = new List<VariableValue>().AsReadOnly();
            }

            //Parse Links

            var properties = runElement.Properties as IDictionary<string, dynamic>;

            if (properties["game"] is string)
            {
                run.GameID = runElement.game as string;
                run.game = new Lazy<Game>(() => client.Games.GetGame(run.GameID));
            }
            else
            {
                var game = Game.Parse(client, properties["game"].data) as Game;
                run.game = new Lazy<Game>(() => game);
                run.GameID = game.ID;
            }

            if (properties["category"] == null)
            {
                run.category = new Lazy<Category>(() => null);
            }
            else if (properties["category"] is string)
            {
                run.CategoryID = runElement.category as string;
                run.category = new Lazy<Category>(() => client.Categories.GetCategory(run.CategoryID));
            }
            else
            {
                var category = Category.Parse(client, properties["category"].data) as Category;
                run.category = new Lazy<Category>(() => category);
                if (category != null)
                    run.CategoryID = category.ID;
            }

            if (properties["level"] == null)
            {
                run.level = new Lazy<Level>(() => null);
            }
            else if (properties["level"] is string)
            {
                run.LevelID = runElement.level as string;
                run.level = new Lazy<Level>(() => client.Levels.GetLevel(run.LevelID));
            }
            else
            {
                var level = Level.Parse(client, properties["level"].data) as Level;
                run.level = new Lazy<Level>(() => level);
                if (level != null)
                    run.LevelID = level.ID;
            }

            if (properties.ContainsKey("platform"))
            {
                var platform = Platform.Parse(client, properties["platform"].data) as Platform;
                run.System.platform = new Lazy<Platform>(() => platform);
            }

            if (properties.ContainsKey("region"))
            {
                var region = Region.Parse(client, properties["region"].data) as Region;
                run.System.region = new Lazy<Region>(() => region);
            }

            if (!string.IsNullOrEmpty(run.Status.ExaminerUserID))
            {
                run.examiner = new Lazy<User>(() => client.Users.GetUser(run.Status.ExaminerUserID));
            }
            else
            {
                run.examiner = new Lazy<User>(() => null);
            }
        }

        public static Run Parse(SpeedrunComClient client, dynamic runElement)
        {
            var run = new Run();

            Parse(run, client, runElement);

            return run;
        }

        public override int GetHashCode()
        {
            return (ID ?? string.Empty).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Run;

            if (other == null)
                return false;

            return ID == other.ID;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} in {2}", Game.Name, Category.Name, Times.Primary);
        }
    }
}
