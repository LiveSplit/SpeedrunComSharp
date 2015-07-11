using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SpeedrunComSharp
{
    public class UsersClient
    {
        public const string Name = "users";

        private SpeedrunComClient baseClient;

        public UsersClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetUsersUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
        }

        public User GetUserFromSiteUri(string siteUri)
        {
            var id = GetUserIDFromSiteUri(siteUri);

            if (string.IsNullOrEmpty(id))
                return null;

            return GetUser(id);
        }

        public string GetUserIDFromSiteUri(string siteUri)
        {
            var elementDescription = SpeedrunComClient.GetElementDescriptionFromSiteUri(siteUri);

            if (elementDescription == null
                || elementDescription.Type != ElementType.User)
                return null;

            return elementDescription.ID;
        }

        public IEnumerable<User> GetUsers(
            string name = null, string twitch = null,
            string hitbox = null, string speedrunslive = null,
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

            if (!string.IsNullOrEmpty(speedrunslive))
                parameters.Add(string.Format("speedrunslive={0}",
                    Uri.EscapeDataString(speedrunslive)));

            parameters.AddRange(orderBy.ToParameters());

            var uri = GetUsersUri(parameters.ToParameters());
            return baseClient.DoPaginatedRequest(uri,
                x => User.Parse(baseClient, x) as User);
        }

        public User GetUser(string userId)
        {
            var uri = GetUsersUri(string.Format("/{0}",
                Uri.EscapeDataString(userId)));

            var result = baseClient.DoRequest(uri);

            return User.Parse(baseClient, result.data);
        }
    }
}
