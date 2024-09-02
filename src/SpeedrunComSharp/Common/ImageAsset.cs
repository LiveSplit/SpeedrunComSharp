using System;

namespace SpeedrunComSharp;

public class ImageAsset
{
    public Uri Uri { get; private set; }

    private ImageAsset() { }

    public static ImageAsset Parse(SpeedrunComClient client, dynamic imageElement)
    {
        if (imageElement == null || imageElement.uri == null)
        {
            return null;
        }

        var image = new ImageAsset();

        var uri = imageElement.uri as string;
        image.Uri = new Uri(uri);

        return image;
    }
}
