using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class Player
    {
        public bool IsUser { get { return string.IsNullOrEmpty(GuestName); } }
        public string UserID { get; private set; }
        public string GuestName { get; private set; }

        #region Links

        private Lazy<User> user;
        private Lazy<Guest> guest;

        public User User { get { return user.Value; } }
        public Guest Guest { get { return guest.Value; } }
        public string Name { get { return IsUser ? User.Name : GuestName; } }

        #endregion

        private Player() { }

        public static Player Parse(SpeedrunComClient client, dynamic playerElement)
        {
            var player = new Player();

            var id = playerElement.id as string;

            if (playerElement.rel as string == "user")
            {
                player.UserID = id;
                player.user = new Lazy<User>(() => client.Users.GetUser(player.UserID));
                player.guest = new Lazy<Guest>(() => null);
            }
            else
            {
                player.GuestName = id;
                player.guest = new Lazy<Guest>(() => client.Guests.GetGuest(player.GuestName));
                player.user = new Lazy<User>(() => null);
            }

            return player;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
