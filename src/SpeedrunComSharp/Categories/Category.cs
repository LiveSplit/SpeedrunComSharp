using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpeedrunComSharp;

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
    private Lazy<Leaderboard> leaderboard;
    private Lazy<Record> worldRecord;

    public string GameID { get; private set; }
    public Game Game => game.Value;
    public ReadOnlyCollection<Variable> Variables => variables.Value;
    public IEnumerable<Run> Runs { get; private set; }
    public Leaderboard Leaderboard => leaderboard.Value;
    public Record WorldRecord => worldRecord.Value;

    #endregion

    private Category() { }

    public static Category Parse(SpeedrunComClient client, dynamic categoryElement)
    {
        if (categoryElement is ArrayList)
        {
            return null;
        }

        var category = new Category
        {
            //Parse Attributes

            ID = categoryElement.id as string,
            Name = categoryElement.name as string,
            WebLink = new Uri(categoryElement.weblink as string),
            Type = categoryElement.type == "per-game" ? CategoryType.PerGame : CategoryType.PerLevel,
            Rules = categoryElement.rules as string,
            Players = Players.Parse(client, categoryElement.players),
            IsMiscellaneous = categoryElement.miscellaneous
        };

        //Parse Links

        var properties = categoryElement.Properties as IDictionary<string, dynamic>;
        var links = properties["links"] as IEnumerable<dynamic>;

        string gameUri = links.First(x => x.rel == "game").uri as string;
        category.GameID = gameUri[(gameUri.LastIndexOf('/') + 1)..];

        if (properties.ContainsKey("game"))
        {
            dynamic gameElement = properties["game"].data;
            var game = Game.Parse(client, gameElement) as Game;
            category.game = new Lazy<Game>(() => game);
        }
        else
        {
            category.game = new Lazy<Game>(() => client.Games.GetGame(category.GameID));
        }

        if (properties.ContainsKey("variables"))
        {
            Variable parser(dynamic x)
            {
                return Variable.Parse(client, x) as Variable;
            }

            dynamic variables = client.ParseCollection(properties["variables"].data, (Func<dynamic, Variable>)parser);
            category.variables = new Lazy<ReadOnlyCollection<Variable>>(() => variables);
        }
        else
        {
            category.variables = new Lazy<ReadOnlyCollection<Variable>>(() => client.Categories.GetVariables(category.ID));
        }

        category.Runs = client.Runs.GetRuns(categoryId: category.ID);

        if (category.Type == CategoryType.PerGame)
        {

            category.leaderboard = new Lazy<Leaderboard>(() =>
                {
                    Leaderboard leaderboard = client.Leaderboards
                                    .GetLeaderboardForFullGameCategory(category.GameID, category.ID);

                    leaderboard.game = new Lazy<Game>(() => category.Game);
                    leaderboard.category = new Lazy<Category>(() => category);

                    foreach (Record record in leaderboard.Records)
                    {
                        record.game = leaderboard.game;
                        record.category = leaderboard.category;
                    }

                    return leaderboard;
                });
            category.worldRecord = new Lazy<Record>(() =>
                {
                    if (category.leaderboard.IsValueCreated)
                    {
                        return category.Leaderboard.Records.FirstOrDefault();
                    }

                    Leaderboard leaderboard = client.Leaderboards
                                    .GetLeaderboardForFullGameCategory(category.GameID, category.ID, top: 1);

                    leaderboard.game = new Lazy<Game>(() => category.Game);
                    leaderboard.category = new Lazy<Category>(() => category);

                    foreach (Record record in leaderboard.Records)
                    {
                        record.game = leaderboard.game;
                        record.category = leaderboard.category;
                    }

                    return leaderboard.Records.FirstOrDefault();
                });
        }
        else
        {
            category.leaderboard = new Lazy<Leaderboard>(() => null);
            category.worldRecord = new Lazy<Record>(() => null);
        }

        return category;
    }

    public override int GetHashCode()
    {
        return (ID ?? string.Empty).GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is not Category other)
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
