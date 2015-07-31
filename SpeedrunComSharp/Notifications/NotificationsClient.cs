using System;
using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public class NotificationsClient
    {
        public const string Name = "notifications";

        private SpeedrunComClient baseClient;

        public NotificationsClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetNotificationsUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
        }

        public IEnumerable<Notification> GetNotifications(
            int? elementsPerPage = null,
            NotificationsOrdering ordering = default(NotificationsOrdering))
        {
            var parameters = new List<string>();

            if (elementsPerPage.HasValue)
                parameters.Add(string.Format("max={0}", elementsPerPage.Value));

            parameters.AddRange(ordering.ToParameters());

            var uri = GetNotificationsUri(string.Format("{0}", 
                parameters.ToParameters()));

            return baseClient.DoPaginatedRequest<Notification>(uri,
                x => Notification.Parse(baseClient, x));
        }
    }
}
