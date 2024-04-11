using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SpeedrunComSharp
{
    public class Notification : IElementWithID
    {
        public string ID { get; private set; }
        public DateTime TimeCreated { get; private set; }
        public NotificationStatus Status { get; private set; }
        public string Text { get; private set; }

        public NotificationType Type { get; private set; }
        public Uri WebLink { get; private set; }

        #region Links

        private Lazy<Run> run;
        private Lazy<Game> game;

        public string RunID { get; private set; }
        public Run Run { get { return run.Value; } }

        public string GameID { get; private set; }
        public Game Game { get { return game.Value; } }

        #endregion

        private Notification() { }

        public static Notification Parse(SpeedrunComClient client, dynamic notificationElement)
        {
            var notification = new Notification();

            //Parse Attributes

            notification.ID = notificationElement.id as string;
            notification.TimeCreated = DateTime.Parse(notificationElement.created as string, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            notification.Status = NotificationStatusHelpers.Parse(notificationElement.status as string);
            notification.Text = notificationElement.text as string;
            notification.Type = NotificationTypeHelpers.Parse(notificationElement.item.rel as string);
            notification.WebLink = new Uri(notificationElement.item.uri as string);

            //Parse Links

            var links = notificationElement.links as IList<dynamic>;

            if (links != null)
            {
                var run = links.FirstOrDefault(x => x.rel == "run");

                if (run != null)
                {
                    var runUri = run.uri as string;
                    notification.RunID = runUri.Substring(runUri.LastIndexOf("/") + 1);
                }

                var game = links.FirstOrDefault(x => x.rel == "game");

                if (game != null)
                {
                    var gameUri = game.uri as string;
                    notification.GameID = gameUri.Substring(gameUri.LastIndexOf("/") + 1);
                }
            }

            if (!string.IsNullOrEmpty(notification.RunID))
            {
                notification.run = new Lazy<Run>(() => client.Runs.GetRun(notification.RunID));
            }
            else
            {
                notification.run = new Lazy<Run>(() => null);
            }

            if (!string.IsNullOrEmpty(notification.GameID))
            {
                notification.game = new Lazy<Game>(() => client.Games.GetGame(notification.GameID));
            }
            else
            {
                notification.game = new Lazy<Game>(() => null);
            }
            

            return notification;
        }
    }
}
