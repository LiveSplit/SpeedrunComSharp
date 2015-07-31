using System;

namespace SpeedrunComSharp
{
    public enum NotificationType
    {
        Post, Run, Game, Guide
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
            }

            throw new ArgumentException("type");
        }
    }
}
