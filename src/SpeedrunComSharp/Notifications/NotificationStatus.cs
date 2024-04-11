using System;

namespace SpeedrunComSharp
{
    public enum NotificationStatus
    {
        Unread, Read
    }

    public static class NotificationStatusHelpers
    {
        public static NotificationStatus Parse(string status)
        {
            switch (status)
            {
                case "read":
                    return NotificationStatus.Read;
                case "unread":
                    return NotificationStatus.Unread;
            }

            throw new ArgumentException("status");
        }
    }
}
