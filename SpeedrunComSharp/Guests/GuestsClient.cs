using System;

namespace SpeedrunComSharp
{
    public class GuestsClient
    {
        public const string Name = "guests";

        private SpeedrunComClient baseClient;

        public GuestsClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetGuestsUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
        }

        /// <summary>
        /// Fetch a Guest object identified by its URI.
        /// </summary>
        /// <param name="siteUri">The site URI for the guest.</param>
        /// <returns></returns>
        public Guest GetGuestFromSiteUri(string siteUri)
        {
            var id = GetGuestIDFromSiteUri(siteUri);

            if (string.IsNullOrEmpty(id))
                return null;

            return GetGuest(id);
        }

        /// <summary>
        /// Fetch a Guest ID identified by its URI.
        /// </summary>
        /// <param name="siteUri">The site URI for the guest.</param>
        /// <returns></returns>
        public string GetGuestIDFromSiteUri(string siteUri)
        {
            var elementDescription = baseClient.GetElementDescriptionFromSiteUri(siteUri);

            if (elementDescription == null
                || elementDescription.Type != ElementType.Guest)
                return null;

            return elementDescription.ID;
        }

        /// <summary>
        /// Fetch a Guest object identified by its name.
        /// </summary>
        /// <param name="guestName">The name of the guest.</param>
        /// <returns></returns>
        public Guest GetGuest(string guestName)
        {
            var uri = GetGuestsUri(string.Format("/{0}", Uri.EscapeDataString(guestName)));
            var result = baseClient.DoRequest(uri);

            return Guest.Parse(baseClient, result.data);
        }
    }
}
