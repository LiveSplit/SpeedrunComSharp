using System;

namespace SpeedrunComSharp;

public enum NotificationType
{
    Post, Run, Game, Guide, Thread, Resource, Moderator
}

public static class NotificationTypeHelpers
{
    public static NotificationType Parse(string type)
    {
        return type switch
        {
            "post" => NotificationType.Post,
            "run" => NotificationType.Run,
            "game" => NotificationType.Game,
            "guide" => NotificationType.Guide,
            "thread" => NotificationType.Thread,
            "resource" => NotificationType.Resource,
            "moderator" => NotificationType.Moderator,
            _ => throw new ArgumentException("type"),
        };
    }
}
