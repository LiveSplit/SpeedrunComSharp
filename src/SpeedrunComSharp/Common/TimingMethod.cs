using System;

namespace SpeedrunComSharp;

public enum TimingMethod
{
    GameTime, RealTime, RealTimeWithoutLoads
}

public static class TimingMethodHelpers
{
    public static string ToAPIString(this TimingMethod timingMethod)
    {
        return timingMethod switch
        {
            TimingMethod.RealTime => "realtime",
            TimingMethod.RealTimeWithoutLoads => "realtime_noloads",
            TimingMethod.GameTime => "ingame",
            _ => throw new ArgumentException("timingMethod"),
        };
    }

    public static TimingMethod FromString(string element)
    {
        return element switch
        {
            "realtime" => TimingMethod.RealTime,
            "realtime_noloads" => TimingMethod.RealTimeWithoutLoads,
            "ingame" => TimingMethod.GameTime,
            _ => throw new ArgumentException("element"),
        };
    }
}
