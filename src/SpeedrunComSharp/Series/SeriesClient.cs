using System;
using System.Collections.Generic;

namespace SpeedrunComSharp;

public class SeriesClient
{
    public const string Name = "series";

    private readonly SpeedrunComClient baseClient;

    public SeriesClient(SpeedrunComClient baseClient)
    {
        this.baseClient = baseClient;
    }

    public static Uri GetSeriesUri(string subUri)
    {
        return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
    }

    /// <summary>
    /// Fetch a Series object identified by its URI.
    /// </summary>
    /// <param name="siteUri">The site URI for the series.</param>
    /// <param name="embeds">Optional. If included, will dictate the embedded resources included in the response.</param>
    /// <returns></returns>
    public Series GetSeriesFromSiteUri(string siteUri, SeriesEmbeds embeds = default(SeriesEmbeds))
    {
        var id = GetSeriesIDFromSiteUri(siteUri);

        if (string.IsNullOrEmpty(id))
            return null;

        return GetSingleSeries(id, embeds);
    }

    /// <summary>
    /// Fetch a Series ID identified by its URI.
    /// </summary>
    /// <param name="siteUri">The site URI for the series.</param>
    /// <returns></returns>
    public string GetSeriesIDFromSiteUri(string siteUri)
    {
        var elementDescription = baseClient.GetElementDescriptionFromSiteUri(siteUri);

        if (elementDescription == null
            || elementDescription.Type != ElementType.Series)
            return null;

        return elementDescription.ID;
    }

    /// <summary>
    /// Fetch a Collection of Series objects identified by the parameters provided.
    /// </summary>
    /// <param name="name">Optional. If included, will filter series by their name.</param>
    /// <param name="abbreviation">Optional. If included, will filter series by their abbreviation.</param>
    /// <param name="moderatorId">Optional. If included, will filter series by their moderators.</param>
    /// <param name="elementsPerPage">Optional. If included, will dictate the amount of elements included in each pagination.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <param name="orderBy">Optional. If omitted, series will be in the same order as the API.</param>
    /// <returns></returns>
    public IEnumerable<Series> GetMultipleSeries(
       string name = null, string abbreviation = null,
       string moderatorId = null, int? elementsPerPage = null,
       SeriesEmbeds embeds = default(SeriesEmbeds),
       SeriesOrdering orderBy = default(SeriesOrdering))
    {
        var parameters = new List<string>() { embeds.ToString() };

        parameters.AddRange(orderBy.ToParameters());

        if (!string.IsNullOrEmpty(name))
            parameters.Add(string.Format("name={0}", Uri.EscapeDataString(name)));

        if (!string.IsNullOrEmpty(abbreviation))
            parameters.Add(string.Format("abbreviation={0}", Uri.EscapeDataString(abbreviation)));

        if (!string.IsNullOrEmpty(moderatorId))
            parameters.Add(string.Format("moderator={0}", Uri.EscapeDataString(moderatorId)));

        if (elementsPerPage.HasValue)
            parameters.Add(string.Format("max={0}", elementsPerPage.Value));

        var uri = GetSeriesUri(parameters.ToParameters());
        return baseClient.DoPaginatedRequest(uri,
            x => Series.Parse(baseClient, x) as Series);
    }

    /// <summary>
    /// Fetch a Series object identified by its ID.
    /// </summary>
    /// <param name="seriesId">The ID of the series.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <returns></returns>
    public Series GetSingleSeries(string seriesId, SeriesEmbeds embeds = default(SeriesEmbeds))
    {
        var parameters = new List<string>() { embeds.ToString() };

        var uri = GetSeriesUri(string.Format("/{0}{1}",
            Uri.EscapeDataString(seriesId),
            parameters.ToParameters()));

        var result = baseClient.DoRequest(uri);

        return Series.Parse(baseClient, result.data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="seriesId">The ID of the series.</param>
    /// <param name="name">Optional. If included, will filter series by their name.</param>
    /// <param name="yearOfRelease">Optional. If included, will filter series by their release year.</param>
    /// <param name="platformId">Optional. If included, will filter series by their platform.</param>
    /// <param name="regionId">Optional. If included, will filter series by their region.</param>
    /// <param name="moderatorId">Optional. If included, will filter series by their moderators.</param>
    /// <param name="elementsPerPage">Optional. If included, will dictate the amount of elements included in each pagination.</param>
    /// <param name="embeds">Optional. If included, will dictate the additional resources embedded in the response.</param>
    /// <param name="orderBy">Optional. If omitted, series will be in the same order as the API.</param>
    /// <returns></returns>
    public IEnumerable<Game> GetGames(
        string seriesId,
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

        var uri = GetSeriesUri(string.Format("/{0}/games{1}",
            Uri.EscapeDataString(seriesId),
            parameters.ToParameters()));

        return baseClient.DoPaginatedRequest(uri,
            x => Game.Parse(baseClient, x) as Game);
    }
}
