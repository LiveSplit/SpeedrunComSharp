using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class ProfileClient
    {
        public const string Name = "profile";

        private SpeedrunComClient baseClient;

        public ProfileClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public static Uri GetProfileUri(string subUri)
        {
            return SpeedrunComClient.GetAPIUri(string.Format("{0}{1}", Name, subUri));
        }

        public User GetProfile()
        {
            var uri = GetProfileUri(string.Empty);

            var result = baseClient.DoRequest(uri);

            return User.Parse(baseClient, result.data);
        }
    }
}
