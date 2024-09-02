using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpeedrunComSharp;

public class GamesClient
{
    public const string Name = "games";

    private readonly SpeedrunComClient baseClient;

    public GamesClient(SpeedrunComClient baseClient)
    {
        this.baseClient = baseClient;
    }

    public static Uri GetGamesUri(string subUri)
    {
        return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
    }

    /// <summary>
    /// Fetch a Game object identified by its URI.
    /// </summary>
    /// <param name="siteUri">The site URI for the game.</param>
    /// <param name="embeds">Optional. If included, will dictate the embedded resources included in the response.</param>
    /// <returns></returns>
    public Game GetGameFromSiteUri(string siteUri, GameEmbeds embeds = default(GameEmbeds))
    {
        var id = GetGameIDFromSiteUri(siteUri);

        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        return GetGame(id, embeds);
    }

    /// <summary>
    /// Fetch a Game ID identified by its URI.
    /// </summary>
    /// <param name="siteUri">The site URI for the game.</param>
    /// <returns></returns>
    public string GetGameIDFromSiteUri(string siteUri)
    {
        var elementDescription = baseClient.GetElementDescriptionFromSiteUri(siteUri);

        if (elementDescription == null
            || elementDescription.Type != ElementType.Game)
        {
            return null;
        }

        return elementDescription.ID;
    }

    /// <summary>
    /// Fetch a Collection of Game objects identified by the parameters provided.
    /// </summary>
    /// <param name="name">Optional. If included, will filter games by their name.</param>
    /// <param name="yearOfRelease">Optional. If included, will filter games by their release year.</param>
    /// <param name="platformId">Optional. If included, will filter games by their platform.</param>
    /// <param name="regionId">Optional. If included, will filter games by their region.</param>
    /// <param name="moderatorId">Optional. If included, will filter games by their moderators.</param>
    /// <param name="elementsPerPage">Optional. If included, will dictate the amount of elements included in each pagination.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <param name="orderBy">Optional. If omitted, games will be in the same order as the API.</param>
    /// <returns></returns>
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
        {
            parameters.Add(string.Format("name={0}", Uri.EscapeDataString(name)));
        }

        if (yearOfRelease.HasValue)
        {
            parameters.Add(string.Format("released={0}", yearOfRelease.Value));
        }

        if (!string.IsNullOrEmpty(platformId))
        {
            parameters.Add(string.Format("platform={0}", Uri.EscapeDataString(platformId)));
        }

        if (!string.IsNullOrEmpty(regionId))
        {
            parameters.Add(string.Format("region={0}", Uri.EscapeDataString(regionId)));
        }

        if (!string.IsNullOrEmpty(moderatorId))
        {
            parameters.Add(string.Format("moderator={0}", Uri.EscapeDataString(moderatorId)));
        }

        if (elementsPerPage.HasValue)
        {
            parameters.Add(string.Format("max={0}", elementsPerPage.Value));
        }

        var uri = GetGamesUri(parameters.ToParameters());
        return baseClient.DoPaginatedRequest(uri,
            x => Game.Parse(baseClient, x) as Game);
    }

    /// <summary>
    /// Fetch a Collection of GameHeader objects.
    /// </summary>
    /// <param name="elementsPerPage">Optional. If included, will dictate the amount of elements included in each pagination.</param>
    /// <param name="orderBy">Optional. If omitted, gameheaders will be in the same order as the API.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Fetch a Game object identified by its ID.
    /// </summary>
    /// <param name="gameId">The ID for the game.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <returns></returns>
    public Game GetGame(string gameId, GameEmbeds embeds = default(GameEmbeds))
    {
        var parameters = new List<string>() { embeds.ToString() };

        var uri = GetGamesUri(string.Format("/{0}{1}",
            Uri.EscapeDataString(gameId),
            parameters.ToParameters()));

        var result = baseClient.DoRequest(uri);

        return Game.Parse(baseClient, result.data);
    }

    /// <summary>
    /// Fetch a Game object identified by its name.
    /// </summary>
    /// <param name="name">The name of the game.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <returns></returns>
    public Game SearchGame(string name, GameEmbeds embeds = default(GameEmbeds))
    {
        var game = GetGames(name: name, embeds: embeds, elementsPerPage: 1).FirstOrDefault();

        return game;
    }

    /// <summary>
    /// Fetch a Game object identified by its name with an exact match.
    /// </summary>
    /// <param name="name">The name of the game.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <returns></returns>
    public Game SearchGameExact(string name, GameEmbeds embeds = default(GameEmbeds))
    {
        var game = GetGames(name: name, embeds: embeds, elementsPerPage: 1).Take(1).FirstOrDefault(x => x.Name == name);

        return game;
    }

    /// <summary>
    /// Fetch a Collection of Category objects from a game's ID.
    /// </summary>
    /// <param name="gameId">The ID for the game.</param>
    /// <param name="miscellaneous">Optional. If included, will dictate whether miscellaneous categories are included.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <param name="orderBy">Optional. If omitted, categories will be in the same order as the API.</param>
    /// <returns></returns>
    public ReadOnlyCollection<Category> GetCategories(
        string gameId, bool miscellaneous = true,
        CategoryEmbeds embeds = default(CategoryEmbeds),
        CategoriesOrdering orderBy = default(CategoriesOrdering))
    {
        var parameters = new List<string>() { embeds.ToString() };

        parameters.AddRange(orderBy.ToParameters());

        if (!miscellaneous)
        {
            parameters.Add("miscellaneous=no");
        }

        var uri = GetGamesUri(string.Format("/{0}/categories{1}",
            Uri.EscapeDataString(gameId),
            parameters.ToParameters()));

        return baseClient.DoDataCollectionRequest(uri,
            x => Category.Parse(baseClient, x) as Category);
    }

    /// <summary>
    /// Fetch a Collection of Level objects from a game's ID.
    /// </summary>
    /// <param name="gameId">The ID for the game.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <param name="orderBy">Optional. If omitted, levels will be in the same order as the API.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Fetch a Collection of Variable objects from a game's ID.
    /// </summary>
    /// <param name="gameId">The ID for the category.</param>
    /// <param name="orderBy">Optional. If omitted, variables will be in the same order as the API.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Fetch a Collection of Game objects derived from a game's ID.
    /// </summary>
    /// <param name="gameId">The ID for the game.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <param name="orderBy">Optional. If omitted, games will be in the same order as the API.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Fetch a Leaderboard object from a game's ID.
    /// </summary>
    /// <param name="gameId">The ID for the game.</param>
    /// <param name="top">Optional. If included, will dictate the amount of top runs included in the response.</param>
    /// <param name="scope">Optional. If included, will dictate the scope of the Leaderboard included in the response.</param>
    /// <param name="includeMiscellaneousCategories">Optional. If included, will dictate whether miscellaneous categories are included.</param>
    /// <param name="skipEmptyLeaderboards">Optional. If included, will dictate whether or not empty leaderboards are included in the response.</param>
    /// <param name="elementsPerPage">Optional. If included, will dictate the amount of elements included in each pagination.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <returns></returns>
    public IEnumerable<Leaderboard> GetRecords(string gameId,
        int? top = null, LeaderboardScope scope = LeaderboardScope.All,
        bool includeMiscellaneousCategories = true, bool skipEmptyLeaderboards = false,
        int? elementsPerPage = null,
        LeaderboardEmbeds embeds = default(LeaderboardEmbeds))
    {
        var parameters = new List<string>() { embeds.ToString() };

        if (top.HasValue)
        {
            parameters.Add(string.Format("top={0}", top.Value));
        }

        if (scope != LeaderboardScope.All)
        {
            parameters.Add(scope.ToParameter());
        }

        if (!includeMiscellaneousCategories)
        {
            parameters.Add("miscellaneous=false");
        }

        if (skipEmptyLeaderboards)
        {
            parameters.Add("skip-empty=true");
        }

        if (elementsPerPage.HasValue)
        {
            parameters.Add(string.Format("max={0}", elementsPerPage.Value));
        }

        var uri = GetGamesUri(string.Format("/{0}/records{1}",
            Uri.EscapeDataString(gameId),
            parameters.ToParameters()));

        return baseClient.DoPaginatedRequest<Leaderboard>(uri,
            x => Leaderboard.Parse(baseClient, x));
    }
}
