using System;

namespace SpeedrunComSharp;

public enum NotificationStatus
{
    Unread, Read
}

public static class NotificationStatusHelpers
{
    public static NotificationStatus Parse(string status)
    {
        return status switch
        {
            "read" => NotificationStatus.Read,
            "unread" => NotificationStatus.Unread,
            _ => throw new ArgumentException("status"),
        };
    }
}
