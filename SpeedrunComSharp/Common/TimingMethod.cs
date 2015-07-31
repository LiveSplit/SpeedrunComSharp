using System;

namespace SpeedrunComSharp
{
    public enum TimingMethod
    {
        GameTime, RealTime, RealTimeWithoutLoads
    }

    public static class TimingMethodHelpers
    {
        public static string ToAPIString(this TimingMethod timingMethod)
        {
            switch (timingMethod)
            {
                case TimingMethod.RealTime:
                    return "realtime";
                case TimingMethod.RealTimeWithoutLoads:
                    return "realtime_noloads";
                case TimingMethod.GameTime:
                    return "ingame";
            }
            throw new ArgumentException("timingMethod");
        }

        public static TimingMethod FromString(string element)
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
    }
}
