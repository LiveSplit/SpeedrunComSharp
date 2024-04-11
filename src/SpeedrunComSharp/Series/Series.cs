using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace SpeedrunComSharp
{
    public class Series : IElementWithID
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string JapaneseName { get; private set; }
        public string Abbreviation { get; private set; }
        public Uri WebLink { get; private set; }
        public DateTime? CreationDate { get; private set; }
        public Assets Assets { get; private set; }

        #region Embeds

        private Lazy<ReadOnlyCollection<User>> moderatorUsers;

        /// <summary>
        /// null when embedded
        /// </summary>
        public ReadOnlyCollection<Moderator> Moderators { get; private set; }

        public ReadOnlyCollection<User> ModeratorUsers { get { return moderatorUsers.Value; } }

        #endregion

        #region Links

        public IEnumerable<Game> Games { get; private set; }

        #endregion

        private Series() { }

        public static Series Parse(SpeedrunComClient client, dynamic seriesElement)
        {
            var series = new Series();

            //Parse Attributes

            series.ID = seriesElement.id as string;
            series.Name = seriesElement.names.international as string;
            series.JapaneseName = seriesElement.names.japanese as string;
            series.WebLink = new Uri(seriesElement.weblink as string);
            series.Abbreviation = seriesElement.abbreviation as string;

            var created = seriesElement.created as string;
            if (!string.IsNullOrEmpty(created))
                series.CreationDate = DateTime.Parse(created, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

            series.Assets = Assets.Parse(client, seriesElement.assets);

            //Parse Embeds

            if (seriesElement.moderators is DynamicJsonObject && seriesElement.moderators.Properties.ContainsKey("data"))
            {
                Func<dynamic, User> userParser = x => User.Parse(client, x) as User;
                ReadOnlyCollection<User> users = client.ParseCollection(seriesElement.moderators.data, userParser);
                series.moderatorUsers = new Lazy<ReadOnlyCollection<User>>(() => users);
            }
            else if (seriesElement.moderators is DynamicJsonObject)
            {
                var moderatorsProperties = seriesElement.moderators.Properties as IDictionary<string, dynamic>;
                series.Moderators = moderatorsProperties.Select(x => Moderator.Parse(client, x)).ToList().AsReadOnly();

                series.moderatorUsers = new Lazy<ReadOnlyCollection<User>>(
                    () =>
                    {
                        ReadOnlyCollection<User> users;

                        if (series.Moderators.Count(x => !x.user.IsValueCreated) > 1)
                        {
                            users = client.Games.GetGame(series.ID, embeds: new GameEmbeds(embedModerators: true)).ModeratorUsers;

                            foreach (var user in users)
                            {
                                var moderator = series.Moderators.FirstOrDefault(x => x.UserID == user.ID);
                                if (moderator != null)
                                {
                                    moderator.user = new Lazy<User>(() => user);
                                }
                            }
                        }
                        else
                        {
                            users = series.Moderators.Select(x => x.User).ToList().AsReadOnly();
                        }

                        return users;
                    });
            }
            else
            {
                series.Moderators = new ReadOnlyCollection<Moderator>(new Moderator[0]);
                series.moderatorUsers = new Lazy<ReadOnlyCollection<User>>(() => new List<User>().AsReadOnly());
            }

            //Parse Links

            series.Games = client.Series.GetGames(series.ID).Select(game =>
                {
                    game.series = new Lazy<Series>(() => series);
                    return game;
                }).Cache();

            return series;
        }

        public override int GetHashCode()
        {
            return (ID ?? string.Empty).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Series;

            if (other == null)
                return false;

            return ID == other.ID;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
