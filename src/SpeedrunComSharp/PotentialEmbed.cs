using System;

namespace SpeedrunComSharp;

internal class PotentialEmbed<T>
    where T : IElementWithID
{
    public Lazy<T> Object { get; private set; }
    public string ID { get; private set; }

    private PotentialEmbed() { }

    public static PotentialEmbed<T> Parse(dynamic element, Func<string, T> objectQuery, Func<dynamic, T> objectParser)
    {
        var potentialEmbed = new PotentialEmbed<T>();

        if (element == null)
        {
            potentialEmbed.Object = new Lazy<T>(() => default);
        }
        else if (element is string)
        {
            potentialEmbed.ID = element as string;
            potentialEmbed.Object = new Lazy<T>(() => objectQuery(potentialEmbed.ID));
        }
        else
        {
            dynamic parsedObject = objectParser(element.data);
            potentialEmbed.Object = new Lazy<T>(() => parsedObject);
            if (parsedObject != null)
            {
                potentialEmbed.ID = parsedObject.ID;
            }
        }

        return potentialEmbed;
    }
}
