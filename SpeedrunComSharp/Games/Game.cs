using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace SpeedrunComSharp
{
    public class Game : IElementWithID
    {
        public GameHeader Header { get; private set; }
        public string ID { get { return Header.ID; } }
        public string Name { get { return Header.Name; } }
        public string JapaneseName { get { return Header.JapaneseName; } }
        public string Abbreviation { get { return Header.Abbreviation; } }
        public Uri WebLink { get { return Header.WebLink; } }
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

        public ReadOnlyCollection<User> ModeratorUsers { get { return moderatorUsers.Value; } }
        public ReadOnlyCollection<Platform> Platforms { get { return platforms.Value; } }
        public ReadOnlyCollection<Region> Regions { get { return regions.Value; } }

        #endregion

        #region Links

        private Lazy<ReadOnlyCollection<Level>> levels;
        private Lazy<ReadOnlyCollection<Category>> categories;
        private Lazy<ReadOnlyCollection<Variable>> variables;
        internal Lazy<Series> series;
        private Lazy<Game> originalGame;
        private Lazy<ReadOnlyCollection<Game>> romHacks;

        public IEnumerable<Run> Runs { get; private set; }
        public ReadOnlyCollection<Level> Levels { get { return levels.Value; } }
        public ReadOnlyCollection<Category> Categories { get { return categories.Value; } }
        public IEnumerable<Category> FullGameCategories { get { return Categories.Where(category => category.Type == CategoryType.PerGame); } }
        public IEnumerable<Category> LevelCategories { get { return Categories.Where(category => category.Type == CategoryType.PerLevel); } }
        public ReadOnlyCollection<Variable> Variables { get { return variables.Value; } }
        public IEnumerable<Variable> FullGameVariables { get { return Variables.Where(variable => variable.Scope.Type == VariableScopeType.FullGame || variable.Scope.Type == VariableScopeType.Global); } }
        public IEnumerable<Variable> LevelVariables { get { return Variables.Where(variable => variable.Scope.Type == VariableScopeType.AllLevels || variable.Scope.Type == VariableScopeType.Global); } }
        public string SeriesID { get; private set; }
        public Series Series { get { return series.Value; } }
        public string OriginalGameID { get; private set; }
        public Game OriginalGame { get { return originalGame.Value; } }
        public ReadOnlyCollection<Game> RomHacks { get { return romHacks.Value; } }

        #endregion

        private Game() { }

        public static Game Parse(SpeedrunComClient client, dynamic gameElement)
        {
            var game = new Game();

            //Parse Attributes

            game.Header = GameHeader.Parse(client, gameElement);
            game.YearOfRelease = gameElement.released;
            game.Ruleset = Ruleset.Parse(client, gameElement.ruleset);

            game.IsRomHack = gameElement.romhack;

            var created = gameElement.created as string;
            if (!string.IsNullOrEmpty(created))
                game.CreationDate = DateTime.Parse(created, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

            game.Assets = Assets.Parse(client, gameElement.assets);

            //Parse Embeds

            var properties = gameElement.Properties as IDictionary<string, dynamic>;

            if (gameElement.moderators is DynamicJsonObject && gameElement.moderators.Properties.ContainsKey("data"))
            {
                Func<dynamic, User> userParser = x => User.Parse(client, x) as User;
                ReadOnlyCollection<User> users = client.ParseCollection(gameElement.moderators.data, userParser);
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
                Func<dynamic, Platform> platformParser = x => Platform.Parse(client, x) as Platform;
                ReadOnlyCollection<Platform> platforms = client.ParseCollection(gameElement.platforms.data, platformParser);
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
                Func<dynamic, Region> regionParser = x => Region.Parse(client, x) as Region;
                ReadOnlyCollection<Region> regions = client.ParseCollection(gameElement.regions.data, regionParser);
                game.regions = new Lazy<ReadOnlyCollection<Region>>(() => regions);
                game.RegionIDs = regions.Select(x => x.ID).ToList().AsReadOnly();
            }

            //Parse Links

            game.Runs = client.Runs.GetRuns(gameId: game.ID);

            if (properties.ContainsKey("levels"))
            {
                Func<dynamic, Level> levelParser = x => Level.Parse(client, x) as Level;
                ReadOnlyCollection<Level> levels = client.ParseCollection(gameElement.levels.data, levelParser);
                game.levels = new Lazy<ReadOnlyCollection<Level>>(() => levels);
            }
            else
            {
                game.levels = new Lazy<ReadOnlyCollection<Level>>(() => client.Games.GetLevels(game.ID));
            }

            if (properties.ContainsKey("categories"))
            {
                Func<dynamic, Category> categoryParser = x => Category.Parse(client, x) as Category;
                ReadOnlyCollection<Category> categories = client.ParseCollection(gameElement.categories.data, categoryParser);
                
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
                Func<dynamic, Variable> variableParser = x => Variable.Parse(client, x) as Variable;
                ReadOnlyCollection<Variable> variables = client.ParseCollection(gameElement.variables.data, variableParser);
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
                return false;

            return ID == other.ID;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
