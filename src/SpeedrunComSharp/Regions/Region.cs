using System.Collections;
using System.Collections.Generic;

namespace SpeedrunComSharp;

public class Region : IElementWithID
{
    public string ID { get; private set; }
    public string Name { get; private set; }

    public string Abbreviation
    {
        get
        {
            return Name switch
            {
                "USA / NTSC" => "NTSC-U",
                "EUR / PAL" => "PAL",
                "JPN / NTSC" => "NTSC-J",
                "CHN / iQue" => "CHN",
                "KOR / NTSC" => "KOR",
                _ => Name,
            };
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
        {
            return null;
        }

        var region = new Region
        {
            //Parse Attributes

            ID = regionElement.id as string,
            Name = regionElement.name as string
        };

        //Parse Links

        region.Games = client.Games.GetGames(regionId: region.ID);
        region.Runs = client.Runs.GetRuns(regionId: region.ID);

        return region;
    }

    public override int GetHashCode()
    {
        return (ID ?? string.Empty).GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is not Region region)
        {
            return false;
        }

        return ID == region.ID;
    }

    public override string ToString()
    {
        return Name;
    }
}
