namespace SpeedrunComSharp;

public class Country
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string JapaneseName { get; private set; }

    private Country() { }

    public static Country Parse(SpeedrunComClient client, dynamic countryElement)
    {
        var country = new Country
        {
            Code = countryElement.code as string,
            Name = countryElement.names.international as string,
            JapaneseName = countryElement.names.japanese as string
        };

        return country;
    }

    public override string ToString()
    {
        return Name;
    }
}
