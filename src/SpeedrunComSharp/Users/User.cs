using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace SpeedrunComSharp;

public class User : IElementWithID
{
    public string ID { get; private set; }
    public string Name { get; private set; }
    public string JapaneseName { get; private set; }
    public string[] Pronouns { get; private set; }
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

    public Uri Icon { get; private set; }
    public Uri Image { get; private set; }

    #region Links

    private Lazy<ReadOnlyCollection<Record>> personalBests;

    public IEnumerable<Run> Runs { get; private set; }
    public IEnumerable<Game> ModeratedGames { get; private set; }
    public ReadOnlyCollection<Record> PersonalBests => personalBests.Value;

    #endregion

    private User() { }

    private static UserRole parseUserRole(string role)
    {
        return role switch
        {
            "banned" => UserRole.Banned,
            "user" => UserRole.User,
            "trusted" => UserRole.Trusted,
            "moderator" => UserRole.Moderator,
            "admin" => UserRole.Admin,
            "programmer" => UserRole.Programmer,
            "contentmoderator" => UserRole.ContentModerator,
            _ => throw new ArgumentException("role"),
        };
    }

    public static User Parse(SpeedrunComClient client, dynamic userElement)
    {
        var user = new User();

        var properties = userElement.Properties as IDictionary<string, dynamic>;

        //Parse Attributes

        user.ID = userElement.id as string;
        user.Name = userElement.names.international as string;
        user.JapaneseName = userElement.names.japanese as string;

        string pronounsTemp = userElement.pronouns as string;
        if (!string.IsNullOrWhiteSpace(pronounsTemp))
        {
            user.Pronouns = pronounsTemp.Split(new string[] { ", " }, StringSplitOptions.None);
        }

        user.WebLink = new Uri(userElement.weblink as string);
        user.NameStyle = UserNameStyle.Parse(client, properties["name-style"]) as UserNameStyle;
        user.Role = parseUserRole(userElement.role as string);

        string signUpDate = userElement.signup as string;
        if (!string.IsNullOrEmpty(signUpDate))
        {
            user.SignUpDate = DateTime.Parse(signUpDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }

        user.Location = Location.Parse(client, userElement.location) as Location;

        dynamic twitchLink = userElement.twitch;
        if (twitchLink != null)
        {
            user.TwitchProfile = new Uri(twitchLink.uri as string);
        }

        dynamic hitboxLink = userElement.hitbox;
        if (hitboxLink != null)
        {
            user.HitboxProfile = new Uri(hitboxLink.uri as string);
        }

        dynamic youtubeLink = userElement.youtube;
        if (youtubeLink != null)
        {
            user.YoutubeProfile = new Uri(youtubeLink.uri as string);
        }

        dynamic twitterLink = userElement.twitter;
        if (twitterLink != null)
        {
            user.TwitterProfile = new Uri(twitterLink.uri as string);
        }

        dynamic speedRunsLiveLink = userElement.speedrunslive;
        if (speedRunsLiveLink != null)
        {
            user.SpeedRunsLiveProfile = new Uri(speedRunsLiveLink.uri as string);
        }

        dynamic iconTemp = userElement.assets.icon.uri;
        if (iconTemp != null)
        {
            user.Icon = new Uri(iconTemp as string);
        }

        dynamic imageTemp = userElement.assets.image.uri;
        if (imageTemp != null)
        {
            user.Image = new Uri(imageTemp as string);
        }

        //Parse Links

        user.Runs = client.Runs.GetRuns(userId: user.ID);
        user.ModeratedGames = client.Games.GetGames(moderatorId: user.ID);
        user.personalBests = new Lazy<ReadOnlyCollection<Record>>(() =>
            {
                ReadOnlyCollection<Record> records = client.Users.GetPersonalBests(userId: user.ID);
                var lazy = new Lazy<User>(() => user);

                foreach (Record record in records)
                {
                    Player player = record.Players.FirstOrDefault(x => x.UserID == user.ID);
                    if (player != null)
                    {
                        player.user = lazy;
                    }
                }

                return records;
            });

        return user;
    }

    public override int GetHashCode()
    {
        return (ID ?? string.Empty).GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is not User other)
        {
            return false;
        }

        return ID == other.ID;
    }

    public override string ToString()
    {
        return Name;
    }
}
