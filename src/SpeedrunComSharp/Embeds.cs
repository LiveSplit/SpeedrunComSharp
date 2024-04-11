using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedrunComSharp
{
    internal struct Embeds
    {
        private Dictionary<string, bool> embedDictionary;

        public bool this[string name]
        {
            get
            {
                MakeSureInit();

                if (embedDictionary.ContainsKey(name))
                    return embedDictionary[name];
                else
                    return false;
            }
            set
            {
                MakeSureInit();

                if (embedDictionary.ContainsKey(name))
                    embedDictionary[name] = value;
                else
                    embedDictionary.Add(name, value);
            }
        }

        private void MakeSureInit()
        {
            if (embedDictionary == null)
                embedDictionary = new Dictionary<string, bool>();
        }

        public override string ToString()
        {
            MakeSureInit();

            if (!embedDictionary.Values.Any(x => x))
                return "";

            return "embed=" +
                embedDictionary
                .Where(x => x.Value)
                .Select(x => Uri.EscapeDataString(x.Key))
                .Aggregate(",");
        }
    }
}
