using System.Collections.Generic;

namespace SpeedrunComSharp
{
    public class Record : Run
    {
        public int Rank { get; private set; }

        private Record() { }
        
        public static new Record Parse(SpeedrunComClient client, dynamic recordElement)
        {
            var record = new Record();

            record.Rank = recordElement.place;

            //Parse potential embeds

            var properties = recordElement.Properties as IDictionary<string, dynamic>;

            if (properties.ContainsKey("game"))
                recordElement.run.game = recordElement.game;
            if (properties.ContainsKey("category"))
                recordElement.run.category = recordElement.category;
            if (properties.ContainsKey("level"))
                recordElement.run.level = recordElement.level;
            if (properties.ContainsKey("players"))
                recordElement.run.players = recordElement.players;
            if (properties.ContainsKey("region"))
                recordElement.run.region = recordElement.region;
            if (properties.ContainsKey("platform"))
                recordElement.run.platform = recordElement.platform;

            Run.Parse(record, client, recordElement.run);

            return record;
        }

        public override int GetHashCode()
        {
            return (ID ?? string.Empty).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Record;

            if (other == null)
                return false;

            return ID == other.ID;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} in {2}", Game.Name, Category.Name, Times.Primary);
        }
    }
}
