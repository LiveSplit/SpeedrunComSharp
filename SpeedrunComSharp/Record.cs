using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class Record : Run
    {
        public int Rank { get; private set; }

        private Record() { }
        
        public static Record Parse(SpeedrunComClient client, dynamic recordElement)
        {
            var record = new Record();

            record.Rank = recordElement.place;
            Run.Parse(record, client, recordElement.run);

            return record;
        }
    }
}
