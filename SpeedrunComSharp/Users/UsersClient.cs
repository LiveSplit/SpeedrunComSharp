using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
            var elementDescription = baseClient.GetElementDescriptionFromSiteUri(siteUri);

            if (elementDescription == null
                || elementDescription.Type != ElementType.User)
                return null;

            return elementDescription.ID;
        }

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

        public User GetUser(string userId)
        {
            var uri = GetUsersUri(string.Format("/{0}",
                Uri.EscapeDataString(userId)));

            var result = baseClient.DoRequest(uri);

            return User.Parse(baseClient, result.data);
        }

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
}
