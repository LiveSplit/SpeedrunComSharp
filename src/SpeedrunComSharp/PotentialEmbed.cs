using System;

namespace SpeedrunComSharp
{
    internal class PotentialEmbed<T>
        where T : IElementWithID
    {
        public Lazy<T> Object { get; private set; }
        public string ID { get; private set; }

        private PotentialEmbed() { }

        public static PotentialEmbed<G> Parse<G>(dynamic element, Func<string, G> objectQuery, Func<dynamic, G> objectParser)
            where G : IElementWithID
        {
            var potentialEmbed = new PotentialEmbed<G>();

            if (element == null)
            {
                potentialEmbed.Object = new Lazy<G>(() => default(G));
            }
            else if (element is string)
            {
                potentialEmbed.ID = element as string;
                potentialEmbed.Object = new Lazy<G>(() => objectQuery(potentialEmbed.ID));
            }
            else
            {
                var parsedObject = objectParser(element.data);
                potentialEmbed.Object = new Lazy<G>(() => parsedObject);
                if (parsedObject != null)
                    potentialEmbed.ID = parsedObject.ID;
            }

            return potentialEmbed;
        }
    }
}
