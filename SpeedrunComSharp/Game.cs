using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class Game : IAPIElementWithID
    {
        public GameHeader Header { get; private set; }
        public string ID { get { return Header.ID; } }
        public string Name { get { return Header.Name; } }
        public string JapaneseName { get { return Header.JapaneseName; } }
        public string Abbreviation { get { return Header.Abbreviation; } }
        public Uri WebLink { get { return Header.WebLink; } }
        public int? YearOfRelease { get; private set; }
        public Ruleset Ruleset { get; private set; }
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
        private Lazy<Game> parent;
        private Lazy<ReadOnlyCollection<Game>> children;
        private Lazy<IDictionary<string, ReadOnlyCollection<Record>>> leaderboards;

        public IEnumerable<Run> Runs { get; private set; }
        public ReadOnlyCollection<Level> Levels { get { return levels.Value; } }
        public ReadOnlyCollection<Category> Categories { get { return categories.Value; } }
        public IEnumerable<Category> FullGameCategories { get { return Categories.Where(category => category.Type == CategoryType.PerGame); } }
        public IEnumerable<Category> LevelCategories { get { return Categories.Where(category => category.Type == CategoryType.PerLevel); } }
        public ReadOnlyCollection<Variable> Variables { get { return variables.Value; } }
        public IEnumerable<Variable> FullGameVariables { get { return Variables.Where(variable => variable.Scope.Type == VariableScopeType.FullGame || variable.Scope.Type == VariableScopeType.Global); } }
        public IEnumerable<Variable> LevelVariables { get { return Variables.Where(variable => variable.Scope.Type == VariableScopeType.AllLevels || variable.Scope.Type == VariableScopeType.Global); } }
        public string ParentGameID { get; private set; }
        public Game Parent { get { return parent.Value; } }
        public ReadOnlyCollection<Game> Children { get { return children.Value; } }
        public IDictionary<string, ReadOnlyCollection<Record>> Leaderboards { get { return leaderboards.Value; } }

        #endregion

        private Game() { }

        public static Game Parse(SpeedrunComClient client, dynamic gameElement)
        {
            var game = new Game();

            //Parse Attributes

            game.Header = GameHeader.Parse(client, gameElement);
            game.YearOfRelease = gameElement.released;
            game.Ruleset = Ruleset.Parse(client, gameElement.ruleset);

            var created = gameElement.created as string;
            if (!string.IsNullOrEmpty(created))
                game.CreationDate = DateTime.Parse(created, CultureInfo.InvariantCulture);

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
                    () => game.Moderators.Select(x => x.User).ToList().AsReadOnly());
            }
            else
            {
                game.Moderators = new ReadOnlyCollection<Moderator>(new Moderator[0]);
                game.moderatorUsers = new Lazy<ReadOnlyCollection<User>>(() => new List<User>().AsReadOnly());
            }

            if (properties["platforms"] is IList<dynamic>)
            {
                game.PlatformIDs = client.ParseCollection<string>(gameElement.platforms);
                game.platforms = new Lazy<ReadOnlyCollection<Platform>>(
                    () => game.PlatformIDs.Select(x => client.Platforms.GetPlatform(x)).ToList().AsReadOnly());
            }
            else
            {
                Func<dynamic, Platform> platformParser = x => Platform.Parse(client, x) as Platform;
                ReadOnlyCollection<Platform> platforms = client.ParseCollection(gameElement.platforms.data, platformParser);
                game.platforms = new Lazy<ReadOnlyCollection<Platform>>(() => platforms);
                game.PlatformIDs = platforms.Select(x => x.ID).ToList().AsReadOnly();
            }

            if (properties["regions"] is IList<dynamic>)
            {
                game.RegionIDs = client.ParseCollection<string>(gameElement.regions);
                game.regions = new Lazy<ReadOnlyCollection<Region>>(
                    () => game.RegionIDs.Select(x => client.Regions.GetRegion(x)).ToList().AsReadOnly());
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
            var parentLink = links.FirstOrDefault(x => x.rel == "parent");
            if (parentLink != null)
            {
                var parentUri = parentLink.uri as string;
                game.ParentGameID = parentUri.Substring(parentUri.LastIndexOf('/') + 1);
                game.parent = new Lazy<Game>(() => client.Games.GetGame(game.ParentGameID));
            }
            else
            {
                game.parent = new Lazy<Game>(() => null);
            }

            game.children = new Lazy<ReadOnlyCollection<Game>>(() => 
                {
                    var children = client.Games.GetChildren(game.ID);
                    
                    if (children != null)
                    {
                        foreach (var child in children)
                        {
                            child.parent = new Lazy<Game>(() => game);
                        }
                    }
                    
                    return children;
                });

            game.leaderboards = new Lazy<IDictionary<string, ReadOnlyCollection<Record>>>(() =>
                {
                    var records = client
                        .Records
                        .GetRecords(gameName: game.Name, amount: RecordsClient.AllRecords);
                    
                    foreach (var record in records)
                    {
                        record.game = new Lazy<Game>(() => game);    
                    }
                    
                    var grouped = records.GroupBy(x => x.CategoryName)
                        .ToDictionary(x => x.Key, x => x.ToList().AsReadOnly());
                    
                    if (game.categories.IsValueCreated)
                    {
                        foreach (var leaderboard in grouped)
                        {
                            var category = game.Categories.First(x => x.Name == leaderboard.Key);
                            foreach (var record in leaderboard.Value)
                            {
                                record.category = new Lazy<Category>(() => category);
                            }
                        }
                    }
                    
                    return grouped;
                });
                 
            return game;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
