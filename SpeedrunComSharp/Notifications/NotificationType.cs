using System;

namespace SpeedrunComSharp
{
    public enum NotificationType
    {
        Post, Run, Game, Guide, Thread, Resource, Moderator
    }

    public static class NotificationTypeHelpers
    {
        public static NotificationType Parse(string type)
        {
            switch (type)
            {
                case "post":
                    return NotificationType.Post;
                case "run":
                    return NotificationType.Run;
                case "game":
                    return NotificationType.Game;
                case "guide":
                    return NotificationType.Guide;
                case "thread":
                    return NotificationType.Thread;
                case "resource":
                    return NotificationType.Resource;
                case "moderator":
                    return NotificationType.Moderator;
            }

            throw new ArgumentException("type");
        }
    }
}
