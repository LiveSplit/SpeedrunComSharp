using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SpeedrunComSharp
{
    public class UsersClient
    {
        private SpeedrunComClient baseClient;

        public UsersClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetUsersUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("users{0}", subUri));
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
