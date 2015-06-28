using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class SpeedrunComClient
    {
        public static readonly Uri BaseUri = new Uri("http://www.speedrun.com/");
        public static readonly Uri APIUri = new Uri(BaseUri, "api/v1/");

        public string UserAgent { get; private set; }
        private Dictionary<Uri, dynamic> Cache { get; set; }
        public int MaxCacheElements { get; private set; }

        public CategoriesClient Categories { get; private set; }
        public GamesClient Games { get; private set; }
        public GuestsClient Guests { get; private set; }
        public LevelsClient Levels { get; private set; }
        public PlatformsClient Platforms { get; private set; }
        public ProfileClient Profile { get; private set; }
        public RecordsClient Records { get; private set; }
        public RegionsClient Regions { get; private set; }
        public RunsClient Runs { get; private set; }
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
            Levels = new LevelsClient(this);
            Platforms = new PlatformsClient(this);
            Profile = new ProfileClient(this);
            Records = new RecordsClient(this);
            Regions = new RegionsClient(this);
            Runs = new RunsClient(this);
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
            dynamic result;

            if (Cache.ContainsKey(uri))
            {
#if DEBUG_WITH_API_CALLS
                Debug.WriteLine(uri.AbsoluteUri, "Cached API Call");
#endif
                result = Cache[uri];
                Cache.Remove(uri);
            }
            else
            {
#if DEBUG_WITH_API_CALLS
                Debug.WriteLine(uri.AbsoluteUri, "Uncached API Call");
#endif
                result = JSON.FromUri(uri, UserAgent);
            }

            Cache.Add(uri, result);

            while (Cache.Count > MaxCacheElements)
                Cache.Remove(Cache.Keys.First());

            return result;
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
