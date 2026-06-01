using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SpeedrunComSharp;

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
    public Run Run => run.Value;

    public string GameID { get; private set; }
    public Game Game => game.Value;

    #endregion

    private Notification() { }

    public static Notification Parse(SpeedrunComClient client, dynamic notificationElement)
    {
        var notification = new Notification
        {
            //Parse Attributes

            ID = notificationElement.id as string,
            TimeCreated = DateTime.Parse(notificationElement.created as string, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
            Status = NotificationStatusHelpers.Parse(notificationElement.status as string),
            Text = notificationElement.text as string,
            Type = NotificationTypeHelpers.Parse(notificationElement.item.rel as string),
            WebLink = new Uri(notificationElement.item.uri as string)
        };

        //Parse Links

        var links = notificationElement.links as IList<dynamic>;

        if (links != null)
        {
            dynamic run = links.FirstOrDefault(x => x.rel == "run");

            if (run != null)
            {
                string runUri = run.uri as string;
                notification.RunID = runUri[(runUri.LastIndexOf("/") + 1)..];
            }

            dynamic game = links.FirstOrDefault(x => x.rel == "game");

            if (game != null)
            {
                string gameUri = game.uri as string;
                notification.GameID = gameUri[(gameUri.LastIndexOf("/") + 1)..];
            }
        }

        notification.run = !string.IsNullOrEmpty(notification.RunID)
            ? new Lazy<Run>(() => client.Runs.GetRun(notification.RunID))
            : new Lazy<Run>(() => null);

        notification.game = !string.IsNullOrEmpty(notification.GameID)
            ? new Lazy<Game>(() => client.Games.GetGame(notification.GameID))
            : new Lazy<Game>(() => null);

        return notification;
    }
}
