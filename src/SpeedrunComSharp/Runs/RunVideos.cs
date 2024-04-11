using System;
using System.Collections.ObjectModel;

namespace SpeedrunComSharp
{
    public class RunVideos
    {
        public string Text { get; private set; }
        public ReadOnlyCollection<Uri> Links { get; private set; }

        private RunVideos() { }

        private static Uri parseVideoLink(dynamic element)
        {
            var videoUri = element.uri as string;
            if (!string.IsNullOrEmpty(videoUri))
            {
                if (!videoUri.StartsWith("http"))
                    videoUri = "http://" + videoUri;

                if (Uri.IsWellFormedUriString(videoUri, UriKind.Absolute))
                    return new Uri(videoUri);
            }

            return null;
        }

        public static RunVideos Parse(SpeedrunComClient client, dynamic videosElement)
        {
            if (videosElement == null)
                return null;

            var videos = new RunVideos();

            videos.Text = videosElement.text as string;

            videos.Links = client.ParseCollection(videosElement.links, new Func<dynamic, Uri>(parseVideoLink));

            return videos;
        }
    }
}
