using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

namespace SpeedrunComSharp
{
    public class SpeedrunComClient
    {
        public static readonly Uri BaseUri = new Uri("http://www.speedrun.com/");
        public static readonly Uri APIUri = new Uri(BaseUri, "api/v1/");
        public const string APIHttpHeaderRelation = "alternate http://www.speedrun.com/api";

        public string UserAgent { get; private set; }
        private Dictionary<Uri, dynamic> Cache { get; set; }
        public int MaxCacheElements { get; private set; }

        public CategoriesClient Categories { get; private set; }
        public GamesClient Games { get; private set; }
        public GuestsClient Guests { get; private set; }
        public LeaderboardsClient Leaderboards { get; private set; }
        public LevelsClient Levels { get; private set; }
        public PlatformsClient Platforms { get; private set; }
        public ProfileClient Profile { get; private set; }
        public RegionsClient Regions { get; private set; }
        public RunsClient Runs { get; private set; }
        public SeriesClient Series { get; private set; }
        public UsersClient Users { get; private set; }
        public VariablesClient Variables { get; private set; }

        public SpeedrunComClient()
            : this(userAgent: "SpeedRunComSharp/1.0")
        { }

        public SpeedrunComClient(string userAgent, int maxCacheElements = 50)
        {
            UserAgent = userAgent;
            MaxCacheElements = maxCacheElements;
            Cache = new Dictionary<Uri, dynamic>();
            Categories = new CategoriesClient(this);
            Games = new GamesClient(this);
            Guests = new GuestsClient(this);
            Leaderboards = new LeaderboardsClient(this);
            Levels = new LevelsClient(this);
            Platforms = new PlatformsClient(this);
            Profile = new ProfileClient(this);
            Regions = new RegionsClient(this);
            Runs = new RunsClient(this);
            Series = new SeriesClient(this);
            Users = new UsersClient(this);
            Variables = new VariablesClient(this);
        }

        public static Uri GetSiteUri(string subUri)
        {
            return new Uri(BaseUri, subUri);
        }

        public static Uri GetAPIUri(string subUri)
        {
            return new Uri(APIUri, subUri);
        }

        public static ElementDescription GetElementDescriptionFromSiteUri(string siteUri)
        {
            try
            {
                var request = WebRequest.Create(siteUri);
                var response = request.GetResponse();
                var linksString = response.Headers["Link"];
                var links = HttpWebLink.ParseLinks(linksString);
                var link = links.FirstOrDefault(x => x.Relation == APIHttpHeaderRelation);

                if (link == null)
                    return null;

                var uri = link.Uri;
                var elementDescription = ElementDescription.ParseUri(uri);

                return elementDescription;
            }
            catch
            {
                return null;
            }
        }

        internal ReadOnlyCollection<T> ParseCollection<T>(dynamic collection, Func<dynamic, T> parser)
        {
            var enumerable = collection as IEnumerable<dynamic>;
            if (enumerable == null)
                return new List<T>(new T[0]).AsReadOnly();

            return enumerable.Select(parser).ToList().AsReadOnly();
        }

        internal ReadOnlyCollection<T> ParseCollection<T>(dynamic collection)
        {
            var enumerable = collection as IEnumerable<dynamic>;
            if (enumerable == null)
                return new List<T>(new T[0]).AsReadOnly();

            return enumerable.OfType<T>().ToList().AsReadOnly();
        }

        internal dynamic DoRequest(Uri uri)
        {
            lock (this)
            {
                dynamic result;

                if (Cache.ContainsKey(uri))
                {
#if DEBUG_WITH_API_CALLS
                Console.WriteLine(uri.AbsoluteUri, "Cached API Call");
#endif
                    result = Cache[uri];
                    Cache.Remove(uri);
                }
                else
                {
#if DEBUG_WITH_API_CALLS
                Console.WriteLine(uri.AbsoluteUri, "Uncached API Call");
#endif
                    try
                    {
                        result = JSON.FromUri(uri, UserAgent);
                    }
                    catch (WebException ex)
                    {
                        try
                        {
                            using (var stream = ex.Response.GetResponseStream())
                            {
                                var json = JSON.FromStream(stream);
                                throw new APIException(json.message);
                            }
                        }
                        catch (APIException ex2)
                        {
                            throw ex2;
                        }
                        catch
                        {
                            throw ex;
                        }
                    }
                }

                Cache.Add(uri, result);

                while (Cache.Count > MaxCacheElements)
                    Cache.Remove(Cache.Keys.First());

                return result;
            }
        }

        internal ReadOnlyCollection<T> DoDataCollectionRequest<T>(Uri uri, Func<dynamic, T> parser)
        {
            var result = DoRequest(uri);
            var elements = result.data as IEnumerable<dynamic>;
            if (elements == null)
                return new ReadOnlyCollection<T>(new T[0]);

            return elements.Select(parser).ToList().AsReadOnly();
        }

        private IEnumerable<T> doPaginatedRequest<T>(Uri uri, Func<dynamic, T> parser)
        {
            do
            {
                var result = DoRequest(uri);

                if (result.pagination.size == 0)
                {
                    yield break;
                }

                var elements = result.data as IEnumerable<dynamic>;

                foreach (var element in elements)
                {
                    yield return parser(element);
                }

                var links = result.pagination.links as IEnumerable<dynamic>;
                if (links == null)
                {
                    yield break;
                }

                var paginationLink = links.FirstOrDefault(x => x.rel == "next");
                if (paginationLink == null)
                {
                    yield break;
                }

                uri = new Uri(paginationLink.uri as string);
            } while (true);
        }

        internal IEnumerable<T> DoPaginatedRequest<T>(Uri uri, Func<dynamic, T> parser)
        {
            return doPaginatedRequest(uri, parser).Cache();
        }
    }
}
