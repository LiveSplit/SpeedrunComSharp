﻿using System;
using System.Collections.Generic;

namespace SpeedrunComSharp;

public class Moderator
{
    public string UserID { get; private set; }
    public ModeratorType Type { get; private set; }

    #region Links

    internal Lazy<User> user;

    public User User => user.Value;
    public string Name => User.Name;

    #endregion

    private Moderator() { }

    public static Moderator Parse(SpeedrunComClient client, KeyValuePair<string, dynamic> moderatorElement)
    {
        var moderator = new Moderator
        {
            UserID = moderatorElement.Key,
            Type = (moderatorElement.Value as string) == "moderator"
            ? ModeratorType.Moderator
            : ModeratorType.SuperModerator
        };

        moderator.user = new Lazy<User>(() => client.Users.GetUser(moderator.UserID));

        return moderator;
    }

    public override string ToString()
    {
        return Name;
    }
}
