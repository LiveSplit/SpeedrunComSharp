using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class Category : IElementWithID
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public Uri WebLink { get; private set; }
        public CategoryType Type { get; private set; }
        public string Rules { get; private set; }
        public Players Players { get; private set; }
        public bool IsMiscellaneous { get; private set; }

        #region Links

        internal Lazy<Game> game;
        private Lazy<ReadOnlyCollection<Variable>> variables;
        private Lazy<ReadOnlyCollection<Record>> leaderboard;
        private Lazy<Record> worldRecord;

        public string GameID { get; private set; }
        public Game Game { get { return game.Value; } }
        public ReadOnlyCollection<Variable> Variables { get { return variables.Value; } }
        public IEnumerable<Run> Runs { get; private set; }
        public ReadOnlyCollection<Record> Leaderboard { get { return leaderboard.Value; } }
        public Record WorldRecord { get { return worldRecord.Value; }}

        #endregion

        private Category() { }

        public static Category Parse(SpeedrunComClient client, dynamic categoryElement)
        {
            if (categoryElement is ArrayList)
                return null;

            var category = new Category();

            //Parse Attributes

            category.ID = categoryElement.id as string;
            category.Name = categoryElement.name as string;
            category.WebLink = new Uri(categoryElement.weblink as string);
            category.Type = categoryElement.type == "per-game" ? CategoryType.PerGame : CategoryType.PerLevel;
            category.Rules = categoryElement.rules as string;
            category.Players = Players.Parse(client, categoryElement.players);
            category.IsMiscellaneous = categoryElement.miscellaneous;

            //Parse Links

            var properties = categoryElement.Properties as IDictionary<string, dynamic>;
            var links = properties["links"] as IEnumerable<dynamic>;

            var gameUri = links.First(x => x.rel == "game").uri as string;
            category.GameID = gameUri.Substring(gameUri.LastIndexOf('/') + 1);

            if (properties.ContainsKey("game"))
            {
                var gameElement = properties["game"].data;
                var game = Game.Parse(client, gameElement) as Game;
                category.game = new Lazy<Game>(() => game);
            }
            else
            {
                category.game = new Lazy<Game>(() => client.Games.GetGame(category.GameID));
            }

            if (properties.ContainsKey("variables"))
            {
                Func<dynamic, Variable> parser = x => Variable.Parse(client, x) as Variable;
                var variables = client.ParseCollection(properties["variables"].data, parser);
                category.variables = new Lazy<ReadOnlyCollection<Variable>>(() => variables);
            }
            else
            {
                category.variables = new Lazy<ReadOnlyCollection<Variable>>(() => client.Categories.GetVariables(category.ID));
            }

            category.Runs = client.Runs.GetRuns(categoryId: category.ID);
            category.leaderboard = new Lazy<ReadOnlyCollection<Record>>(() => 
                {                                                        
                    var leaderboard = client
                                        .Records
                                        .GetRecords(gameName: category.Game.Name, amount: RecordsClient.AllRecords)
                                        .Where(x => x.CategoryName == category.Name)
                                        .ToList()
                                        .AsReadOnly();
                    
                    foreach (var record in leaderboard)
                    {
                        record.category = new Lazy<Category>(() => category);
                        record.game = category.game;
                    }
                    
                    return leaderboard;
                });
            
            category.worldRecord = new Lazy<Record>(() =>
                {
                    if (category.leaderboard.IsValueCreated)
                    {
                        var record = category.Leaderboard.First();
                        
                        record.category = new Lazy<Category>(() => category);
                        record.game = category.game;
                        
                        return record;
                    }
                    else
                        return client.Records.GetWorldRecord(category.Game.Name, category.Name);
                });

            return category;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
