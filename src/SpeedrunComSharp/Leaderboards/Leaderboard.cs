using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpeedrunComSharp;

public class Leaderboard
{
    public Uri WebLink { get; private set; }
    public EmulatorsFilter EmulatorFilter { get; private set; }
    public bool AreRunsWithoutVideoFilteredOut { get; private set; }
    public TimingMethod? OrderedBy { get; private set; }
    public ReadOnlyCollection<VariableValue> VariableFilters { get; private set; }

    public ReadOnlyCollection<Record> Records { get; private set; }

    #region Embeds

    private Lazy<ReadOnlyCollection<Player>> players;
    private Lazy<ReadOnlyCollection<Region>> usedRegions;
    private Lazy<ReadOnlyCollection<Platform>> usedPlatforms;
    private Lazy<ReadOnlyCollection<Variable>> applicableVariables;

    public ReadOnlyCollection<Player> Players => players.Value;
    public ReadOnlyCollection<Region> UsedRegions => usedRegions.Value;
    public ReadOnlyCollection<Platform> UsedPlatforms => usedPlatforms.Value;
    public ReadOnlyCollection<Variable> ApplicableVariables => applicableVariables.Value;

    #endregion

    #region Links

    internal Lazy<Game> game;
    internal Lazy<Category> category;
    private Lazy<Level> level;
    private Lazy<Platform> platformFilter;
    private Lazy<Region> regionFilter;

    public string GameID { get; private set; }
    public Game Game => game.Value;
    public string CategoryID { get; private set; }
    public Category Category => category.Value;
    public string LevelID { get; private set; }
    public Level Level => level.Value;
    public string PlatformIDOfFilter { get; private set; }
    public Platform PlatformFilter => platformFilter.Value;
    public string RegionIDOfFilter { get; private set; }
    public Region RegionFilter => regionFilter.Value;

    #endregion

    private Leaderboard() { }

    public static Leaderboard Parse(SpeedrunComClient client, dynamic leaderboardElement)
    {
        var leaderboard = new Leaderboard();

        var properties = leaderboardElement.Properties as IDictionary<string, dynamic>;

        //Parse Attributes

        leaderboard.WebLink = new Uri(leaderboardElement.weblink as string);

        var emulators = leaderboardElement.emulators as string;
        if (emulators == "true")
        {
            leaderboard.EmulatorFilter = EmulatorsFilter.OnlyEmulators;
        }
        else if (emulators == "false")
        {
            leaderboard.EmulatorFilter = EmulatorsFilter.NoEmulators;
        }
        else
        {
            leaderboard.EmulatorFilter = EmulatorsFilter.NotSet;
        }

        leaderboard.AreRunsWithoutVideoFilteredOut = properties["video-only"];

        //TODO Not actually optional
        if (leaderboardElement.timing != null)
        {
            leaderboard.OrderedBy = TimingMethodHelpers.FromString(leaderboardElement.timing as string);
        }

        if (leaderboardElement.values is DynamicJsonObject)
        {
            var valueProperties = leaderboardElement.values.Properties as IDictionary<string, dynamic>;
            leaderboard.VariableFilters = valueProperties.Select(x => VariableValue.ParseValueDescriptor(client, x) as VariableValue).ToList().AsReadOnly();
        }
        else
        {
            leaderboard.VariableFilters = new List<VariableValue>().AsReadOnly();
        }

        Record recordParser(dynamic x)
        {
            return Record.Parse(client, x) as Record;
        }

        leaderboard.Records = client.ParseCollection(leaderboardElement.runs, (Func<dynamic, Record>)recordParser);

        //Parse Links

        if (properties["game"] is string)
        {
            leaderboard.GameID = leaderboardElement.game as string;
            leaderboard.game = new Lazy<Game>(() => client.Games.GetGame(leaderboard.GameID));
        }
        else
        {
            var game = Game.Parse(client, properties["game"].data) as Game;
            leaderboard.game = new Lazy<Game>(() => game);
            leaderboard.GameID = game.ID;
        }

        if (properties["category"] is string)
        {
            leaderboard.CategoryID = leaderboardElement.category as string;
            leaderboard.category = new Lazy<Category>(() => client.Categories.GetCategory(leaderboard.CategoryID));
        }
        else
        {
            var category = Category.Parse(client, properties["category"].data) as Category;
            leaderboard.category = new Lazy<Category>(() => category);
            if (category != null)
            {
                leaderboard.CategoryID = category.ID;
            }
        }

        if (properties["level"] == null)
        {
            leaderboard.level = new Lazy<Level>(() => null);
        }
        else if (properties["level"] is string)
        {
            leaderboard.LevelID = leaderboardElement.level as string;
            leaderboard.level = new Lazy<Level>(() => client.Levels.GetLevel(leaderboard.LevelID));
        }
        else
        {
            var level = Level.Parse(client, properties["level"].data) as Level;
            leaderboard.level = new Lazy<Level>(() => level);
            if (level != null)
            {
                leaderboard.LevelID = level.ID;
            }
        }

        if (properties["platform"] == null)
        {
            leaderboard.platformFilter = new Lazy<Platform>(() => null);
        }
        else if (properties["platform"] is string)
        {
            leaderboard.PlatformIDOfFilter = properties["platform"] as string;
            leaderboard.platformFilter = new Lazy<Platform>(() => client.Platforms.GetPlatform(leaderboard.PlatformIDOfFilter));
        }
        else
        {
            var platform = Platform.Parse(client, properties["platform"].data) as Platform;
            leaderboard.platformFilter = new Lazy<Platform>(() => platform);
            if (platform != null)
            {
                leaderboard.PlatformIDOfFilter = platform.ID;
            }
        }

        if (properties["region"] == null)
        {
            leaderboard.regionFilter = new Lazy<Region>(() => null);
        }
        else if (properties["region"] is string)
        {
            leaderboard.RegionIDOfFilter = properties["region"] as string;
            leaderboard.regionFilter = new Lazy<Region>(() => client.Regions.GetRegion(leaderboard.RegionIDOfFilter));
        }
        else
        {
            var region = Region.Parse(client, properties["region"].data) as Region;
            leaderboard.regionFilter = new Lazy<Region>(() => region);
            if (region != null)
            {
                leaderboard.RegionIDOfFilter = region.ID;
            }
        }

        //Parse Embeds

        if (properties.ContainsKey("players"))
        {
            Player playerParser(dynamic x)
            {
                return Player.Parse(client, x) as Player;
            }

            var players = client.ParseCollection(leaderboardElement.players.data, (Func<dynamic, Player>)playerParser) as ReadOnlyCollection<Player>;

            foreach (var record in leaderboard.Records)
            {
                record.Players = record.Players.Select(x => players.FirstOrDefault(y => x.Equals(y))).ToList().AsReadOnly();
            }

            leaderboard.players = new Lazy<ReadOnlyCollection<Player>>(() => players);
        }
        else
        {
            leaderboard.players = new Lazy<ReadOnlyCollection<Player>>(() => leaderboard.Records.SelectMany(x => x.Players).ToList().Distinct().ToList().AsReadOnly());
        }

        if (properties.ContainsKey("regions"))
        {
            Region regionParser(dynamic x)
            {
                return Region.Parse(client, x) as Region;
            }

            var regions = client.ParseCollection(leaderboardElement.regions.data, (Func<dynamic, Region>)regionParser) as ReadOnlyCollection<Region>;

            foreach (var record in leaderboard.Records)
            {
                record.System.region = new Lazy<Region>(() => regions.FirstOrDefault(x => x.ID == record.System.RegionID));
            }

            leaderboard.usedRegions = new Lazy<ReadOnlyCollection<Region>>(() => regions);
        }
        else
        {
            leaderboard.usedRegions = new Lazy<ReadOnlyCollection<Region>>(() => leaderboard.Records.Select(x => x.Region).Distinct().Where(x => x != null).ToList().AsReadOnly());
        }

        if (properties.ContainsKey("platforms"))
        {
            Platform platformParser(dynamic x)
            {
                return Platform.Parse(client, x) as Platform;
            }

            var platforms = client.ParseCollection(leaderboardElement.platforms.data, (Func<dynamic, Platform>)platformParser) as ReadOnlyCollection<Platform>;

            foreach (var record in leaderboard.Records)
            {
                record.System.platform = new Lazy<Platform>(() => platforms.FirstOrDefault(x => x.ID == record.System.PlatformID));
            }

            leaderboard.usedPlatforms = new Lazy<ReadOnlyCollection<Platform>>(() => platforms);
        }
        else
        {
            leaderboard.usedPlatforms = new Lazy<ReadOnlyCollection<Platform>>(() => leaderboard.Records.Select(x => x.Platform).Distinct().Where(x => x != null).ToList().AsReadOnly());
        }

        void patchVariablesOfRecords(ReadOnlyCollection<Variable> variables)
        {
            foreach (var record in leaderboard.Records)
            {
                foreach (var value in record.VariableValues)
                {
                    value.variable = new Lazy<Variable>(() => variables.FirstOrDefault(x => x.ID == value.VariableID));
                }
            }
        }

        if (properties.ContainsKey("variables"))
        {
            Variable variableParser(dynamic x)
            {
                return Variable.Parse(client, x) as Variable;
            }

            var variables = client.ParseCollection(leaderboardElement.variables.data, (Func<dynamic, Variable>)variableParser) as ReadOnlyCollection<Variable>;

            patchVariablesOfRecords(variables);

            leaderboard.applicableVariables = new Lazy<ReadOnlyCollection<Variable>>(() => variables);
        }
        else if (string.IsNullOrEmpty(leaderboard.LevelID))
        {
            leaderboard.applicableVariables = new Lazy<ReadOnlyCollection<Variable>>(() =>
                {
                    var variables = leaderboard.Category.Variables;

                    patchVariablesOfRecords(variables);

                    return variables;
                });
        }
        else
        {
            leaderboard.applicableVariables = new Lazy<ReadOnlyCollection<Variable>>(() =>
                {
                    var variables = leaderboard.Category.Variables.Concat(leaderboard.Level.Variables).ToList().Distinct().ToList().AsReadOnly();

                    patchVariablesOfRecords(variables);

                    return variables;
                });
        }

        return leaderboard;
    }
}
