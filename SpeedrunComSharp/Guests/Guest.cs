using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class Guest
    {
        public string Name { get; private set; }

        #region Links

        public IEnumerable<Run> Runs { get; private set; }

        #endregion

        private Guest() { }

        public static Guest Parse(SpeedrunComClient client, dynamic guestElement)
        {
            var guest = new Guest();

            guest.Name = guestElement.name;
            guest.Runs = client.Runs.GetRuns(guestName: guest.Name);

            return guest;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
