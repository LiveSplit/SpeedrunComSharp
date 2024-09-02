using System;

namespace SpeedrunComSharp;

public class VariablesClient
{
    public const string Name = "variables";

    private SpeedrunComClient baseClient;

    public VariablesClient(SpeedrunComClient baseClient)
    {
        this.baseClient = baseClient;
    }

    public static Uri GetVariablesUri(string subUri)
    {
        return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
    }

    /// <summary>
    /// Fetch a Variable object identified by its URI.
    /// </summary>
    /// <param name="siteUri">The site URI of the variable.</param>
    /// <returns></returns>
    public Variable GetVariableFromSiteUri(string siteUri)
    {
        var id = GetVariableIDFromSiteUri(siteUri);

        if (string.IsNullOrEmpty(id))
            return null;

        return GetVariable(id);
    }

    /// <summary>
    /// Fetch a Variable ID identified by its URI.
    /// </summary>
    /// <param name="siteUri">The site URI of the variable.</param>
    /// <returns></returns>
    public string GetVariableIDFromSiteUri(string siteUri)
    {
        var elementDescription = baseClient.GetElementDescriptionFromSiteUri(siteUri);

        if (elementDescription == null
            || elementDescription.Type != ElementType.Variable)
            return null;

        return elementDescription.ID;
    }

    /// <summary>
    /// Fetch a Variable object identified by its ID.
    /// </summary>
    /// <param name="variableId">The ID of the variable.</param>
    /// <returns></returns>
    public Variable GetVariable(string variableId)
    {
        var uri = GetVariablesUri(string.Format("/{0}",
            Uri.EscapeDataString(variableId)));

        var result = baseClient.DoRequest(uri);

        return Variable.Parse(baseClient, result.data);
    }
}
