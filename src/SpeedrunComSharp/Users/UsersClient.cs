using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpeedrunComSharp;

public class UsersClient
{
    public const string Name = "users";

    private readonly SpeedrunComClient baseClient;

    public UsersClient(SpeedrunComClient baseClient)
    {
        this.baseClient = baseClient;
    }

    public static Uri GetUsersUri(string subUri)
    {
        return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
    }

    /// <summary>
    /// Fetch a User object identified by its URI.
    /// </summary>
    /// <param name="siteUri">The site URI for the user.</param>
    /// <returns></returns>
    public User GetUserFromSiteUri(string siteUri)
    {
        var id = GetUserIDFromSiteUri(siteUri);

        if (string.IsNullOrEmpty(id))
            return null;

        return GetUser(id);
    }

    /// <summary>
    /// Fetch a User ID identified by its URI.
    /// </summary>
    /// <param name="siteUri">The site URI for the user.</param>
    /// <returns></returns>
    public string GetUserIDFromSiteUri(string siteUri)
    {
        var elementDescription = baseClient.GetElementDescriptionFromSiteUri(siteUri);

        if (elementDescription == null
            || elementDescription.Type != ElementType.User)
            return null;

        return elementDescription.ID;
    }

    /// <summary>
    /// Fetch a Collection of User objects identified by the parameters provided.
    /// </summary>
    /// <param name="name">Optional. If included, will filter users by their name.</param>
    /// <param name="twitch">Optional. If included, will filter users by their linked Twitch account username.</param>
    /// <param name="hitbox">Optional. If included, will filter users by their linked Hitbox account username.</param>
    /// <param name="twitter">Optional. If included, will filter users by their linked Twitter account username.></param>
    /// <param name="speedrunslive">Optional. If included, will filter users by their linked SpeedrunsLive account username.</param>
    /// <param name="elementsPerPage">Optional. If included, will dictate the amount of elements included in each pagination.</param>
    /// <param name="orderBy">Optional. If omitted, users will be in the same order as the API.</param>
    /// <returns></returns>
    public IEnumerable<User> GetUsers(
        string name = null,
        string twitch = null, string hitbox = null,
        string twitter = null, string speedrunslive = null,
        int? elementsPerPage = null,
        UsersOrdering orderBy = default(UsersOrdering))
    {
        var parameters = new List<string>();

        if (!string.IsNullOrEmpty(name))
            parameters.Add(string.Format("name={0}",
                Uri.EscapeDataString(name)));

        if (!string.IsNullOrEmpty(twitch))
            parameters.Add(string.Format("twitch={0}",
                Uri.EscapeDataString(twitch)));

        if (!string.IsNullOrEmpty(hitbox))
            parameters.Add(string.Format("hitbox={0}",
                Uri.EscapeDataString(hitbox)));

        if (!string.IsNullOrEmpty(twitter))
            parameters.Add(string.Format("twitter={0}",
                Uri.EscapeDataString(twitter)));

        if (!string.IsNullOrEmpty(speedrunslive))
            parameters.Add(string.Format("speedrunslive={0}",
                Uri.EscapeDataString(speedrunslive)));

        if (elementsPerPage.HasValue)
            parameters.Add(string.Format("max={0}", elementsPerPage));

        parameters.AddRange(orderBy.ToParameters());

        var uri = GetUsersUri(parameters.ToParameters());
        return baseClient.DoPaginatedRequest(uri,
            x => User.Parse(baseClient, x) as User);
    }

    /// <summary>
    /// Fetch a Collection of User objects identified by their fuzzy (vague) username.
    /// </summary>
    /// <param name="fuzzyName">Optional. If included, dictates the fuzzy name of the user.</param>
    /// <param name="elementsPerPage">Optional. If included, will dictate the amount of elements included in each pagination.</param>
    /// <param name="orderBy">Optional. If omitted, users will be in the same order as the API.</param>
    /// <returns></returns>
    public IEnumerable<User> GetUsersFuzzy(
        string fuzzyName = null,
        int? elementsPerPage = null,
        UsersOrdering orderBy = default(UsersOrdering))
    {
        var parameters = new List<string>();

        if (!string.IsNullOrEmpty(fuzzyName))
            parameters.Add(string.Format("lookup={0}",
                Uri.EscapeDataString(fuzzyName)));

        if (elementsPerPage.HasValue)
            parameters.Add(string.Format("max={0}", elementsPerPage));

        parameters.AddRange(orderBy.ToParameters());

        var uri = GetUsersUri(parameters.ToParameters());
        return baseClient.DoPaginatedRequest(uri,
            x => User.Parse(baseClient, x) as User);
    }

    /// <summary>
    /// Fetch a User object identified by its ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns></returns>
    public User GetUser(string userId)
    {
        var uri = GetUsersUri(string.Format("/{0}",
            Uri.EscapeDataString(userId)));

        var result = baseClient.DoRequest(uri);

        return User.Parse(baseClient, result.data);
    }

    /// <summary>
    /// Fetch a Collection of Record objects of a user's personal bests identified by their ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="top">Optional. If included, will dictate the amount of top runs included in the response.</param>
    /// <param name="seriesId">Optional. If included, will filter runs by their targetted game's series ID.</param>
    /// <param name="gameId">Optional. If included, will filter runs by their targetted game's ID.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <returns></returns>
    public ReadOnlyCollection<Record> GetPersonalBests(
        string userId, int? top = null,
        string seriesId = null, string gameId = null,
        RunEmbeds embeds = default(RunEmbeds))
    {
        var parameters = new List<string>() { embeds.ToString() };

        if (top.HasValue)
            parameters.Add(string.Format("top={0}", top.Value));
        if (!string.IsNullOrEmpty(seriesId))
            parameters.Add(string.Format("series={0}", Uri.EscapeDataString(seriesId)));
        if (!string.IsNullOrEmpty(gameId))
            parameters.Add(string.Format("game={0}", Uri.EscapeDataString(gameId)));

        var uri = GetUsersUri(string.Format("/{0}/personal-bests{1}",
            Uri.EscapeDataString(userId),
            parameters.ToParameters()));

        return baseClient.DoDataCollectionRequest(uri,
            x => Record.Parse(baseClient, x) as Record);
    }
}
