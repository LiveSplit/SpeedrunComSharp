using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class Record
    {
        public int Rank { get; private set; }
        public Run Run { get; private set; }

        private Record() { }

        public static Record Parse(SpeedrunComClient client, dynamic recordElement)
        {
            var record = new Record();

            record.Rank = recordElement.place;
            record.Run = Run.Parse(client, recordElement.run) as Run;

            return record;
        }
    }
}
