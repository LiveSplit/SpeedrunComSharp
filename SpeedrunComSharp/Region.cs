using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class Region : IElementWithID
    {
        public string ID { get; private set; }
        public string Name { get; private set; }

        public string Abbreviation
        {
            get
            {
                switch (Name)
                {
                    case "USA / NTSC": return "NTSC-U";
                    case "EUR / PAL": return "PAL";
                    case "JPN / NTSC": return "NTSC-J";
                    case "CHN / iQue": return "CHN";
                    case "KOR / NTSC": return "KOR";
                }

                return Name;
            }
        }

        #region Links

        public IEnumerable<Game> Games { get; private set; }
        public IEnumerable<Run> Runs { get; private set; }

        #endregion

        private Region() { }

        public static Region Parse(SpeedrunComClient client, dynamic regionElement)
        {
            if (regionElement is ArrayList)
                return null;

            var region = new Region();

            //Parse Attributes

            region.ID = regionElement.id as string;
            region.Name = regionElement.name as string;

            //Parse Links

            region.Games = client.Games.GetGames(regionId: region.ID);
            region.Runs = client.Runs.GetRuns(regionId: region.ID);

            return region;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
