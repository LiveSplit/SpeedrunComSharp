using System;
using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public class Moderator
    {
        public string UserID { get; private set; }
        public ModeratorType Type { get; private set; }

        #region Links

        internal Lazy<User> user;
        
        public User User { get { return user.Value; } }
        public string Name { get { return User.Name; } }

        #endregion

        private Moderator() { }

        public static Moderator Parse(SpeedrunComClient client, KeyValuePair<string, dynamic> moderatorElement)
        {
            var moderator = new Moderator();

            moderator.UserID = moderatorElement.Key;
            moderator.Type = moderatorElement.Value as string == "moderator" 
                ? ModeratorType.Moderator 
                : ModeratorType.SuperModerator;

            moderator.user = new Lazy<User>(() => client.Users.GetUser(moderator.UserID));

            return moderator;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
