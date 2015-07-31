using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpeedrunComSharp
{
    internal class HttpWebLink
    {
        public string Uri { get; private set; }
        public string Relation { get; private set; }
        public string Anchor { get; private set; }
        public string RelationTypes { get; private set; }
        public string Language { get; private set; }
        public string Media { get; private set; }
        public string Title { get; private set; }
        public string Titles { get; private set; }
        public string Type { get; private set; }

        private HttpWebLink() { }

        public static ReadOnlyCollection<HttpWebLink> ParseLinks(string linksString)
        {
            return (linksString ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => ParseLink(x.Trim(' ')))
                .ToList()
                .AsReadOnly();
        }

        public static HttpWebLink ParseLink(string linkString)
        {
            var link = new HttpWebLink();

            var leftAngledParenthesis = linkString.IndexOf('<');
            var rightAngledParenthesis = linkString.IndexOf('>');

            if (leftAngledParenthesis >= 0 && rightAngledParenthesis >= 0)
            {
                link.Uri = linkString.Substring(leftAngledParenthesis + 1, rightAngledParenthesis - leftAngledParenthesis - 1);
            }

            linkString = linkString.Substring(rightAngledParenthesis + 1);
            var parameters = linkString.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var parameter in parameters)
            {
                var splits = parameter.Split(new[] { '=' }, 2);
                if (splits.Length == 2)
                {
                    var parameterType = splits[0];
                    var parameterValue = splits[1].Trim('"');

                    switch (parameterType)
                    {
                        case "rel":
                            link.Relation = parameterValue;
                            break;
                        case "anchor":
                            link.Anchor = parameterValue;
                            break;
                        case "rev":
                            link.RelationTypes = parameterValue;
                            break;
                        case "hreflang":
                            link.Language = parameterValue;
                            break;
                        case "media":
                            link.Media = parameterValue;
                            break;
                        case "title":
                            link.Title = parameterValue;
                            break;
                        case "title*":
                            link.Titles = parameterValue;
                            break;
                        case "type":
                            link.Type = parameterValue;
                            break;
                    }
                }
            }

            return link;
        }
    }
}
