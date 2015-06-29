using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class Ruleset
    {
        public bool ShowMilliseconds { get; private set; }
        public bool RequiresVerification { get; private set; }
        public bool RequiresVideo { get; private set; }
        public ReadOnlyCollection<TimingMethod> TimingMethods { get; private set; }
        public TimingMethod DefaultTimingMethod { get; private set; }
        public bool EmulatorsAllowed { get; private set; }

        private Ruleset() { }

        private static TimingMethod parseTimingMethod(string element)
        {
            switch (element)
            {
                case "realtime":
                    return TimingMethod.RealTime;
                case "realtime_noloads":
                    return TimingMethod.RealTimeWithoutLoads;
                case "ingame":
                    return TimingMethod.GameTime;
            }

            throw new ArgumentException("element");
        }

        public static Ruleset Parse(SpeedrunComClient client, dynamic rulesetElement)
        {
            var ruleset = new Ruleset();

            var properties = rulesetElement.Properties as IDictionary<string, dynamic>;

            ruleset.ShowMilliseconds = properties["show-milliseconds"];
            ruleset.RequiresVerification = properties["require-verification"];
            ruleset.RequiresVideo = properties["require-video"];

            Func<dynamic, TimingMethod> timingMethodParser = x => parseTimingMethod(x as string);
            ruleset.TimingMethods = client.ParseCollection(properties["run-times"], timingMethodParser);
            ruleset.DefaultTimingMethod = parseTimingMethod(properties["default-time"]);

            ruleset.EmulatorsAllowed = properties["emulators-allowed"];

            return ruleset;
        }

        public override string ToString()
        {
            var list = new List<string>();
            if (ShowMilliseconds)
                list.Add("Show Milliseconds");
            if (RequiresVerification)
                list.Add("Requires Verification");
            if (RequiresVideo)
                list.Add("Requires Video");
            if (EmulatorsAllowed)
                list.Add("Emulators Allowed");
            if (!list.Any())
                list.Add("No Rules");

            return list.Aggregate(", ");
        }
    }
}
