using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace SpeedrunComSharp;

public class Game : IElementWithID
{
    public GameHeader Header { get; private set; }
    public string ID => Header.ID;
    public string Name => Header.Name;
    public string JapaneseName => Header.JapaneseName;
    public string TwitchName => Header.TwitchName;
    public string Abbreviation => Header.Abbreviation;
    public Uri WebLink => Header.WebLink;
    public DateTime? ReleaseDate { get; private set; }
    public int? YearOfRelease { get; private set; }
    public Ruleset Ruleset { get; private set; }
    public bool IsRomHack { get; private set; }
    public DateTime? CreationDate { get; private set; }
    public Assets Assets { get; private set; }

    #region Embeds

    private Lazy<ReadOnlyCollection<User>> moderatorUsers;
    private Lazy<ReadOnlyCollection<Platform>> platforms;
    private Lazy<ReadOnlyCollection<Region>> regions;

    public ReadOnlyCollection<string> PlatformIDs { get; private set; }
    public ReadOnlyCollection<string> RegionIDs { get; private set; }
    /// <summary>
    /// null when embedded
    /// </summary>
    public ReadOnlyCollection<Moderator> Moderators { get; private set; }

    public ReadOnlyCollection<User> ModeratorUsers => moderatorUsers.Value;
    public ReadOnlyCollection<Platform> Platforms => platforms.Value;
    public ReadOnlyCollection<Region> Regions => regions.Value;

    #endregion

    #region Links

    private Lazy<ReadOnlyCollection<Level>> levels;
    private Lazy<ReadOnlyCollection<Category>> categories;
    private Lazy<ReadOnlyCollection<Variable>> variables;
    internal Lazy<Series> series;
    private Lazy<Game> originalGame;
    private Lazy<ReadOnlyCollection<Game>> romHacks;

    public IEnumerable<Run> Runs { get; private set; }
    public ReadOnlyCollection<Level> Levels => levels.Value;
    public ReadOnlyCollection<Category> Categories => categories.Value;
    public IEnumerable<Category> FullGameCategories => Categories.Where(category => category.Type == CategoryType.PerGame);
    public IEnumerable<Category> LevelCategories => Categories.Where(category => category.Type == CategoryType.PerLevel);
    public ReadOnlyCollection<Variable> Variables => variables.Value;
    public IEnumerable<Variable> FullGameVariables => Variables.Where(variable => variable.Scope.Type == VariableScopeType.FullGame || variable.Scope.Type == VariableScopeType.Global);
    public IEnumerable<Variable> LevelVariables => Variables.Where(variable => variable.Scope.Type == VariableScopeType.AllLevels || variable.Scope.Type == VariableScopeType.Global);
    public string SeriesID { get; private set; }
    public Series Series => series.Value;
    public string OriginalGameID { get; private set; }
    public Game OriginalGame => originalGame.Value;
    public ReadOnlyCollection<Game> RomHacks => romHacks.Value;

    #endregion

    private Game() { }

    public static Game Parse(SpeedrunComClient client, dynamic gameElement)
    {
        var game = new Game();
        var gProperties = gameElement.Properties as IDictionary<string, dynamic>;
        //Parse Attributes

        game.Header = GameHeader.Parse(client, gameElement);

        var releaseDate = gProperties["release-date"];
        if (!string.IsNullOrEmpty(releaseDate))
        {
            game.ReleaseDate = DateTime.Parse(releaseDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }

        game.YearOfRelease = gameElement.released;
        game.Ruleset = Ruleset.Parse(client, gameElement.ruleset);

        game.IsRomHack = gameElement.romhack;

        var created = gameElement.created as string;
        if (!string.IsNullOrEmpty(created))
        {
            game.CreationDate = DateTime.Parse(created, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }

        game.Assets = Assets.Parse(client, gameElement.assets);

        //Parse Embeds

        var properties = gameElement.Properties as IDictionary<string, dynamic>;

        if (gameElement.moderators is DynamicJsonObject && gameElement.moderators.Properties.ContainsKey("data"))
        {
            User userParser(dynamic x)
            {
                return User.Parse(client, x) as User;
            }

            ReadOnlyCollection<User> users = client.ParseCollection(gameElement.moderators.data, (Func<dynamic, User>)userParser);
            game.moderatorUsers = new Lazy<ReadOnlyCollection<User>>(() => users);
        }
        else if (gameElement.moderators is DynamicJsonObject)
        {
            var moderatorsProperties = gameElement.moderators.Properties as IDictionary<string, dynamic>;
            game.Moderators = moderatorsProperties.Select(x => Moderator.Parse(client, x)).ToList().AsReadOnly();

            game.moderatorUsers = new Lazy<ReadOnlyCollection<User>>(
                () =>
                {
                    ReadOnlyCollection<User> users;

                    if (game.Moderators.Count(x => !x.user.IsValueCreated) > 1)
                    {
                        users = client.Games.GetGame(game.ID, embeds: new GameEmbeds(embedModerators: true)).ModeratorUsers;

                        foreach (var user in users)
                        {
                            var moderator = game.Moderators.FirstOrDefault(x => x.UserID == user.ID);
                            if (moderator != null)
                            {
                                moderator.user = new Lazy<User>(() => user);
                            }
                        }
                    }
                    else
                    {
                        users = game.Moderators.Select(x => x.User).ToList().AsReadOnly();
                    }

                    return users;
                });
        }
        else
        {
            game.Moderators = new ReadOnlyCollection<Moderator>(new Moderator[0]);
            game.moderatorUsers = new Lazy<ReadOnlyCollection<User>>(() => new List<User>().AsReadOnly());
        }

        if (properties["platforms"] is IList)
        {
            game.PlatformIDs = client.ParseCollection<string>(gameElement.platforms);

            if (game.PlatformIDs.Count > 1)
            {
                game.platforms = new Lazy<ReadOnlyCollection<Platform>>(
                    () => client.Games.GetGame(game.ID, embeds: new GameEmbeds(embedPlatforms: true)).Platforms);
            }
            else
            {
                game.platforms = new Lazy<ReadOnlyCollection<Platform>>(
                    () => game.PlatformIDs.Select(x => client.Platforms.GetPlatform(x)).ToList().AsReadOnly());
            }
        }
        else
        {
            Platform platformParser(dynamic x)
            {
                return Platform.Parse(client, x) as Platform;
            }

            ReadOnlyCollection<Platform> platforms = client.ParseCollection(gameElement.platforms.data, (Func<dynamic, Platform>)platformParser);
            game.platforms = new Lazy<ReadOnlyCollection<Platform>>(() => platforms);
            game.PlatformIDs = platforms.Select(x => x.ID).ToList().AsReadOnly();
        }

        if (properties["regions"] is IList)
        {
            game.RegionIDs = client.ParseCollection<string>(gameElement.regions);

            if (game.RegionIDs.Count > 1)
            {
                game.regions = new Lazy<ReadOnlyCollection<Region>>(
                    () => client.Games.GetGame(game.ID, embeds: new GameEmbeds(embedRegions: true)).Regions);
            }
            else
            {
                game.regions = new Lazy<ReadOnlyCollection<Region>>(
                    () => game.RegionIDs.Select(x => client.Regions.GetRegion(x)).ToList().AsReadOnly());
            }
        }
        else
        {
            Region regionParser(dynamic x)
            {
                return Region.Parse(client, x) as Region;
            }

            ReadOnlyCollection<Region> regions = client.ParseCollection(gameElement.regions.data, (Func<dynamic, Region>)regionParser);
            game.regions = new Lazy<ReadOnlyCollection<Region>>(() => regions);
            game.RegionIDs = regions.Select(x => x.ID).ToList().AsReadOnly();
        }

        //Parse Links

        game.Runs = client.Runs.GetRuns(gameId: game.ID);

        if (properties.ContainsKey("levels"))
        {
            Level levelParser(dynamic x)
            {
                return Level.Parse(client, x) as Level;
            }

            ReadOnlyCollection<Level> levels = client.ParseCollection(gameElement.levels.data, (Func<dynamic, Level>)levelParser);
            game.levels = new Lazy<ReadOnlyCollection<Level>>(() => levels);
        }
        else
        {
            game.levels = new Lazy<ReadOnlyCollection<Level>>(() => client.Games.GetLevels(game.ID));
        }

        if (properties.ContainsKey("categories"))
        {
            Category categoryParser(dynamic x)
            {
                return Category.Parse(client, x) as Category;
            }

            ReadOnlyCollection<Category> categories = client.ParseCollection(gameElement.categories.data, (Func<dynamic, Category>)categoryParser);

            foreach (var category in categories)
            {
                category.game = new Lazy<Game>(() => game);
            }

            game.categories = new Lazy<ReadOnlyCollection<Category>>(() => categories);
        }
        else
        {
            game.categories = new Lazy<ReadOnlyCollection<Category>>(() =>
                {
                    var categories = client.Games.GetCategories(game.ID);

                    foreach (var category in categories)
                    {
                        category.game = new Lazy<Game>(() => game);
                    }

                    return categories;
                });
        }

        if (properties.ContainsKey("variables"))
        {
            Variable variableParser(dynamic x)
            {
                return Variable.Parse(client, x) as Variable;
            }

            ReadOnlyCollection<Variable> variables = client.ParseCollection(gameElement.variables.data, (Func<dynamic, Variable>)variableParser);
            game.variables = new Lazy<ReadOnlyCollection<Variable>>(() => variables);
        }
        else
        {
            game.variables = new Lazy<ReadOnlyCollection<Variable>>(() => client.Games.GetVariables(game.ID));
        }

        var links = properties["links"] as IEnumerable<dynamic>;
        var seriesLink = links.FirstOrDefault(x => x.rel == "series");
        if (seriesLink != null)
        {
            var parentUri = seriesLink.uri as string;
            game.SeriesID = parentUri.Substring(parentUri.LastIndexOf('/') + 1);
            game.series = new Lazy<Series>(() => client.Series.GetSingleSeries(game.SeriesID));
        }
        else
        {
            game.series = new Lazy<Series>(() => null);
        }

        var originalGameLink = links.FirstOrDefault(x => x.rel == "game");
        if (originalGameLink != null)
        {
            var originalGameUri = originalGameLink.uri as string;
            game.OriginalGameID = originalGameUri.Substring(originalGameUri.LastIndexOf('/') + 1);
            game.originalGame = new Lazy<Game>(() => client.Games.GetGame(game.OriginalGameID));
        }
        else
        {
            game.originalGame = new Lazy<Game>(() => null);
        }

        game.romHacks = new Lazy<ReadOnlyCollection<Game>>(() =>
            {
                var romHacks = client.Games.GetRomHacks(game.ID);

                if (romHacks != null)
                {
                    foreach (var romHack in romHacks)
                    {
                        romHack.originalGame = new Lazy<Game>(() => game);
                    }
                }

                return romHacks;
            });

        return game;
    }

    public override int GetHashCode()
    {
        return (ID ?? string.Empty).GetHashCode();
    }

    public override bool Equals(object obj)
    {
        var other = obj as Game;

        if (other == null)
        {
            return false;
        }

        return ID == other.ID;
    }

    public override string ToString()
    {
        return Name;
    }
}
