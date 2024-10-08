﻿namespace SpeedrunComSharp;

public class CountryRegion
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string JapaneseName { get; private set; }

    private CountryRegion() { }

    public static CountryRegion Parse(SpeedrunComClient client, dynamic regionElement)
    {
        var region = new CountryRegion
        {
            Code = regionElement.code as string,
            Name = regionElement.names.international as string,
            JapaneseName = regionElement.names.japanese as string
        };

        return region;
    }

    public override string ToString()
    {
        return Name;
    }
}
