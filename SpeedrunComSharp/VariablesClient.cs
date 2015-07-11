using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SpeedrunComSharp
{
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

        public Variable GetVariableFromSiteUri(string siteUri)
        {
            var id = GetVariableIDFromSiteUri(siteUri);

            if (string.IsNullOrEmpty(id))
                return null;

            return GetVariable(id);
        }

        public string GetVariableIDFromSiteUri(string siteUri)
        {
            var elementDescription = SpeedrunComClient.GetElementDescriptionFromSiteUri(siteUri);

            if (elementDescription == null
                || elementDescription.Type != ElementType.Variable)
                return null;

            return elementDescription.ID;
        }

        public Variable GetVariable(string variableId)
        {
            var uri = GetVariablesUri(string.Format("/{0}",
                Uri.EscapeDataString(variableId)));

            var result = baseClient.DoRequest(uri);

            return Variable.Parse(baseClient, result.data);
        }
    }
}
