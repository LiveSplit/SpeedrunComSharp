#if __MonoCS__
#define MONO
#else
#define DOTNET
#endif

#if DOTNET

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace SpeedrunComSharp.Tests
{
    [TestClass]
    public class CategoryTests
    {
        private SpeedrunComClient client = new SpeedrunComClient("SpeedrunComSharpUnitTests/1.0");

        [TestMethod]
        public void GetASingleCategoryWithoutEmbeds()
        {
            var gameHeaders = client.Games.GetGameHeaders().First();

            var region = client.Regions.GetRegions(elementsPerPage: 1).First();
            var gameInRegion = region.Games.First();

            var platform = client.Platforms.GetPlatforms(elementsPerPage: 1).First();
            var gameOnPlatform = platform.Games.First();

            var gameIn1992 = client.Games.GetGames(yearOfRelease: 1992).First();

            var game = client.Games.SearchGame(name: "Wind Waker");

            var gameJName = game.JapaneseName;
            var gameAbbrev = game.Abbreviation;
            var webLink = game.WebLink;
            var year = game.YearOfRelease;
            var ruleset = game.Ruleset;
            var date = game.CreationDate;
            var assets = game.Assets;
            var moderators = game.ModeratorUsers;
            var gamePlatforms = game.Platforms;
            var gameRegions = game.Regions;

            var category = game.LevelCategories.First();
            var variables = category.Variables;
            var wr = game.FullGameCategories.First().WorldRecord;
            client.Categories.GetCategory(game.LevelCategories.First().ID, new CategoryEmbeds(true, true));

            var toString = category.ToString();

            game = category.Game;
            var gameId = category.GameID;
            var categoryId = category.ID;
            var isMisc = category.IsMiscellaneous;
            var leaderboard = category.Leaderboard;
            var backToCategory = leaderboard.First().Category;
            var backToGame = leaderboard.First().Game;
            var name = category.Name;
            var playersType = category.Players.Type;
            var playersValue = category.Players.Value;
            var rules = category.Rules;
            var runs = category.Runs.Take(30).ToList();
            var categoryType = category.Type;
            variables = category.Variables;
            wr = category.WorldRecord;
            backToCategory = wr.Category;

            var categoryWithEmbeds = client.Categories.GetCategory("pmker8d6", new CategoryEmbeds(true, true));
            game = categoryWithEmbeds.Game;
            variables = categoryWithEmbeds.Variables;
        }
    }
}

#endif