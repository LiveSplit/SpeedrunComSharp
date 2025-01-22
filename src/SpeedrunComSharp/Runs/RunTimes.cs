using System;

namespace SpeedrunComSharp;

public struct RunTimeInformation
{
    public TimeSpan Time;
    public bool HasSubseconds; // Indicator whether the leaderboard shows milliseconds

    public RunTimeInformation(string isoRunTime, double numericRunTime)
    {
        Time = TimeSpan.FromSeconds(numericRunTime);
        HasSubseconds = isoRunTime.Contains(".");
    }

    public static implicit operator TimeSpan(RunTimeInformation instance)
    {
        // For backwards compatibility return the time if queried for a TimeSpan
        return instance.Time;
    }

    public override string ToString()
    {
        return Time.ToString();
    }
}

public class RunTimes
{
    public RunTimeInformation? Primary { get; private set; }
    public RunTimeInformation? RealTime { get; private set; }
    public RunTimeInformation? RealTimeWithoutLoads { get; private set; }
    public RunTimeInformation? GameTime { get; private set; }

    private RunTimes() { }

    public static RunTimes Parse(SpeedrunComClient client, dynamic timesElement)
    {
        var times = new RunTimes();

        if (timesElement.primary != null)
        {
            times.Primary = new RunTimeInformation((string)timesElement.primary, (double)timesElement.primary_t);
        }

        if (timesElement.realtime != null)
        {
            times.RealTime = new RunTimeInformation((string)timesElement.realtime, (double)timesElement.realtime_t);
        }

        if (timesElement.realtime_noloads != null)
        {
            times.RealTimeWithoutLoads = new RunTimeInformation((string)timesElement.realtime_noloads, (double)timesElement.realtime_noloads_t);
        }

        if (timesElement.ingame != null)
        {
            times.GameTime = new RunTimeInformation((string)timesElement.ingame, (double)timesElement.ingame_t);
        }

        return times;

    }

    public override string ToString()
    {
        if (Primary.HasValue)
        {
            return Primary.Value.Time.ToString();
        }
        else
        {
            return "-";
        }
    }
}
