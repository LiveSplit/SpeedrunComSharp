using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class User : IAPIElementWithID
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string JapaneseName { get; private set; }
        public Uri WebLink { get; private set; }
        public UserNameStyle NameStyle { get; private set; }
        public UserRole Role { get; private set; }
        public DateTime? SignUpDate { get; private set; }
        public Location Location { get; private set; }

        public Uri TwitchProfile { get; private set; }
        public Uri HitboxProfile { get; private set; }
        public Uri YoutubeProfile { get; private set; }
        public Uri TwitterProfile { get; private set; }
        public Uri SpeedRunsLiveProfile { get; private set; }

        #region Links

        private Lazy<ReadOnlyCollection<Record>> records;
            
        public IEnumerable<Run> Runs { get; private set; }
        public IEnumerable<Game> ModeratedGames { get; private set; }
        public ReadOnlyCollection<Record> Records { get { return records.Value; } }

        #endregion

        private User() { }

        private static UserRole parseUserRole(string role)
        {
            switch (role)
            {
                case "banned":
                    return UserRole.Banned;
                case "user":
                    return UserRole.User;
                case "trusted":
                    return UserRole.Trusted;
                case "moderator":
                    return UserRole.Moderator;
                case "admin":
                    return UserRole.Admin;
                case "programmer":
                    return UserRole.Programmer;
            }

            throw new ArgumentException("role");
        }

        public static User Parse(SpeedrunComClient client, dynamic userElement)
        {
            var user = new User();

            var properties = userElement.Properties as IDictionary<string, dynamic>;
            var links = properties["links"] as IEnumerable<dynamic>;

            //Parse Attributes

            user.ID = userElement.id as string;
            user.Name = userElement.names.international as string;
            user.JapaneseName = userElement.names.japanese as string;
            user.WebLink = new Uri(userElement.weblink as string);
            user.NameStyle = UserNameStyle.Parse(client, properties["name-style"]) as UserNameStyle;
            user.Role = parseUserRole(userElement.role as string);

            var signUpDate = userElement.signup as string;
            if (!string.IsNullOrEmpty(signUpDate))
                user.SignUpDate = DateTime.Parse(signUpDate, CultureInfo.InvariantCulture);

            user.Location = Location.Parse(client, userElement.location) as Location;

            var twitchLink = userElement.twitch;
            if (twitchLink != null)
                user.TwitchProfile = new Uri(twitchLink.uri as string);

            var hitboxLink = userElement.hitbox;
            if (hitboxLink != null)
                user.HitboxProfile = new Uri(hitboxLink.uri as string);

            var youtubeLink = userElement.youtube;
            if (youtubeLink != null)
                user.YoutubeProfile = new Uri(youtubeLink.uri as string);

            var twitterLink = userElement.twitter;
            if (twitterLink != null)
                user.TwitterProfile = new Uri(twitterLink.uri as string);

            var speedRunsLiveLink = userElement.speedrunslive;
            if (speedRunsLiveLink != null)
                user.SpeedRunsLiveProfile = new Uri(speedRunsLiveLink.uri as string);

            //Parse Links

            user.Runs = client.Runs.GetRuns(userId: user.ID);
            user.ModeratedGames = client.Games.GetGames(moderatorId: user.ID);
            user.records = new Lazy<ReadOnlyCollection<Record>>(() => client.Records.GetRecords(userName: user.Name));

            return user;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
