using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpeedrunComSharp
{
    public class GamesClient
    {
        public const string Name = "games";

        private SpeedrunComClient baseClient;

        public GamesClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetGamesUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
        }

        public Game GetGameFromSiteUri(string siteUri, GameEmbeds embeds = default(GameEmbeds))
        {
            var id = GetGameIDFromSiteUri(siteUri);

            if (string.IsNullOrEmpty(id))
                return null;

            return GetGame(id, embeds);
        }

        public string GetGameIDFromSiteUri(string siteUri)
        {
            var elementDescription = baseClient.GetElementDescriptionFromSiteUri(siteUri);

            if (elementDescription == null
                || elementDescription.Type != ElementType.Game)
                return null;

            return elementDescription.ID;
        }

        public IEnumerable<Game> GetGames(
            string name = null, int? yearOfRelease = null, 
            string platformId = null, string regionId = null, 
            string moderatorId = null, int? elementsPerPage = null,
            GameEmbeds embeds = default(GameEmbeds),
            GamesOrdering orderBy = default(GamesOrdering))
        {
            var parameters = new List<string>() { embeds.ToString() };

            parameters.AddRange(orderBy.ToParameters());

            if (!string.IsNullOrEmpty(name))
                parameters.Add(string.Format("name={0}", Uri.EscapeDataString(name)));

            if (yearOfRelease.HasValue)
                parameters.Add(string.Format("released={0}", yearOfRelease.Value));

            if (!string.IsNullOrEmpty(platformId))
                parameters.Add(string.Format("platform={0}", Uri.EscapeDataString(platformId)));

            if (!string.IsNullOrEmpty(regionId))
                parameters.Add(string.Format("region={0}", Uri.EscapeDataString(regionId)));

            if (!string.IsNullOrEmpty(moderatorId))
                parameters.Add(string.Format("moderator={0}", Uri.EscapeDataString(moderatorId)));

            if (elementsPerPage.HasValue)
                parameters.Add(string.Format("max={0}", elementsPerPage.Value));

            var uri = GetGamesUri(parameters.ToParameters());
            return baseClient.DoPaginatedRequest(uri, 
                x => Game.Parse(baseClient, x) as Game);
        }

        public IEnumerable<GameHeader> GetGameHeaders(int elementsPerPage = 1000,
            GamesOrdering orderBy = default(GamesOrdering))
        {
            var parameters = new List<string>() { "_bulk=yes" };

            parameters.AddRange(orderBy.ToParameters());
            parameters.Add(string.Format("max={0}", elementsPerPage));

            var uri = GetGamesUri(parameters.ToParameters());

            return baseClient.DoPaginatedRequest(uri,
                x => GameHeader.Parse(baseClient, x) as GameHeader);
        }

        public Game GetGame(string gameId, GameEmbeds embeds = default(GameEmbeds))
        {
            var parameters = new List<string>() { embeds.ToString() };

            var uri = GetGamesUri(string.Format("/{0}{1}", 
                Uri.EscapeDataString(gameId), 
                parameters.ToParameters()));

            var result = baseClient.DoRequest(uri);

            return Game.Parse(baseClient, result.data);
        }
        
        public Game SearchGame(string name, GameEmbeds embeds = default(GameEmbeds))
        {
            var game = GetGames(name: name, embeds: embeds, elementsPerPage: 1).FirstOrDefault();
            
            return game;
        }

        public Game SearchGameExact(string name, GameEmbeds embeds = default(GameEmbeds))
        {
            var game = GetGames(name: name, embeds: embeds, elementsPerPage: 1).Take(1).FirstOrDefault(x => x.Name == name);

            return game;
        }

        public ReadOnlyCollection<Category> GetCategories(
            string gameId, bool miscellaneous = true,
            CategoryEmbeds embeds = default(CategoryEmbeds),
            CategoriesOrdering orderBy = default(CategoriesOrdering))
        {
            var parameters = new List<string>() { embeds.ToString() };

            parameters.AddRange(orderBy.ToParameters());

            if (!miscellaneous)
                parameters.Add("miscellaneous=no");

            var uri = GetGamesUri(string.Format("/{0}/categories{1}", 
                Uri.EscapeDataString(gameId), 
                parameters.ToParameters()));

            return baseClient.DoDataCollectionRequest(uri,
                x => Category.Parse(baseClient, x) as Category);
        }

        public ReadOnlyCollection<Level> GetLevels(string gameId,
            LevelEmbeds embeds = default(LevelEmbeds),
            LevelsOrdering orderBy = default(LevelsOrdering))
        {
            var parameters = new List<string>() { embeds.ToString() };

            parameters.AddRange(orderBy.ToParameters());

            var uri = GetGamesUri(string.Format("/{0}/levels{1}", 
                Uri.EscapeDataString(gameId),
                parameters.ToParameters()));

            return baseClient.DoDataCollectionRequest(uri,
                 x => Level.Parse(baseClient, x) as Level);
        }

        public ReadOnlyCollection<Variable> GetVariables(string gameId,
            VariablesOrdering orderBy = default(VariablesOrdering))
        {
            var parameters = new List<string>(orderBy.ToParameters());

            var uri = GetGamesUri(string.Format("/{0}/variables{1}", 
                Uri.EscapeDataString(gameId),
                parameters.ToParameters()));

            return baseClient.DoDataCollectionRequest(uri,
                x => Variable.Parse(baseClient, x) as Variable);
        }

        public ReadOnlyCollection<Game> GetRomHacks(string gameId,
            GameEmbeds embeds = default(GameEmbeds),
            GamesOrdering orderBy = default(GamesOrdering))
        {
            var parameters = new List<string>() { embeds.ToString() };

            parameters.AddRange(orderBy.ToParameters());

            var uri = GetGamesUri(string.Format("/{0}/romhacks{1}", 
                Uri.EscapeDataString(gameId),
                parameters.ToParameters()));

            return baseClient.DoDataCollectionRequest(uri, 
                x => Game.Parse(baseClient, x) as Game);
        }

        public IEnumerable<Leaderboard> GetRecords(string gameId,
            int? top = null, LeaderboardScope scope = LeaderboardScope.All,
            bool includeMiscellaneousCategories = true, bool skipEmptyLeaderboards = false,
            int? elementsPerPage = null,
            LeaderboardEmbeds embeds = default(LeaderboardEmbeds))
        {
            var parameters = new List<string>() { embeds.ToString() };

            if (top.HasValue)
                parameters.Add(string.Format("top={0}", top.Value));
            if (scope != LeaderboardScope.All)
                parameters.Add(scope.ToParameter());
            if (!includeMiscellaneousCategories)
                parameters.Add("miscellaneous=false");
            if (skipEmptyLeaderboards)
                parameters.Add("skip-empty=true");
            if (elementsPerPage.HasValue)
                parameters.Add(string.Format("max={0}", elementsPerPage.Value));

            var uri = GetGamesUri(string.Format("/{0}/records{1}",
                Uri.EscapeDataString(gameId),
                parameters.ToParameters()));

            return baseClient.DoPaginatedRequest<Leaderboard>(uri,
                x => Leaderboard.Parse(baseClient, x));
        }
    }
}
