using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;

namespace SpeedrunComSharp
{
    public class SpeedrunComClient
    {
        public static readonly Uri BaseUri = new Uri("https://www.speedrun.com/");
        public static readonly Uri APIUri = new Uri(BaseUri, "api/v1/");
        public const string APIHttpHeaderRelation = "alternate https://www.speedrun.com/api";

        public string AccessToken { internal get; set; }

        public bool IsAccessTokenValid
        {
            get
            {
                if (AccessToken == null)
                    return false;

                try
                {
                    var profile = Profile;
                    return true;
                }
                catch { }

                return false;
            }
        }

        public string UserAgent { get; private set; }
        private Dictionary<Uri, dynamic> Cache { get; set; }
        public int MaxCacheElements { get; private set; }

        public TimeSpan Timeout { get; private set; }

        public CategoriesClient Categories { get; private set; }
        public GamesClient Games { get; private set; }
        public GuestsClient Guests { get; private set; }
        public LeaderboardsClient Leaderboards { get; private set; }
        public LevelsClient Levels { get; private set; }
        public NotificationsClient Notifications { get; private set; }
        public PlatformsClient Platforms { get; private set; }
        public RegionsClient Regions { get; private set; }
        public RunsClient Runs { get; private set; }
        public SeriesClient Series { get; private set; }
        public UsersClient Users { get; private set; }
        public VariablesClient Variables { get; private set; }

        public User Profile
        {
            get
            {
                var uri = GetProfileUri(string.Empty);
                var result = DoRequest(uri);
                return User.Parse(this, result.data);
            }
        }

        public SpeedrunComClient(string userAgent = "SpeedRunComSharp/1.0", 
            string accessToken = null, int maxCacheElements = 50,
            TimeSpan? timeout = null)
        {
            Timeout = timeout ?? TimeSpan.FromSeconds(30);

            UserAgent = userAgent;
            MaxCacheElements = maxCacheElements;
            AccessToken = accessToken;
            Cache = new Dictionary<Uri, dynamic>();
            Categories = new CategoriesClient(this);
            Games = new GamesClient(this);
            Guests = new GuestsClient(this);
            Leaderboards = new LeaderboardsClient(this);
            Levels = new LevelsClient(this);
            Notifications = new NotificationsClient(this);
            Platforms = new PlatformsClient(this);
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

        public static Uri GetProfileUri(string subUri)
        {
            return GetAPIUri(string.Format("profile{0}", subUri));
        }

        public ElementDescription GetElementDescriptionFromSiteUri(string siteUri)
        {
            try
            {
                var request = WebRequest.Create(siteUri);
                request.Timeout = (int)Timeout.TotalMilliseconds;
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

        internal APIException ParseException(Stream stream)
        {
            var json = JSON.FromStream(stream);
            var properties = json.Properties as IDictionary<string, dynamic>;
            if (properties.ContainsKey("errors"))
            {
                var errors = json.errors as IList<dynamic>;
                return new APIException(json.message as string, errors.Select(x => x as string));
            }
            else
                return new APIException(json.message as string);
        }

        internal dynamic DoPostRequest(Uri uri, string postBody)
        {
            try
            {
                return JSON.FromUriPost(uri, UserAgent, AccessToken, Timeout, postBody);
            }
            catch (WebException ex)
            {
                try
                {
                    using (var stream = ex.Response.GetResponseStream())
                    {
                        throw ParseException(stream);
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

        internal dynamic DoRequest(Uri uri)
        {
            lock (this)
            {
                dynamic result;

                if (Cache.ContainsKey(uri))
                {
#if DEBUG_WITH_API_CALLS
                Console.WriteLine("Cached API Call: {0}", uri.AbsoluteUri);
#endif
                    result = Cache[uri];
                    Cache.Remove(uri);
                }
                else
                {
#if DEBUG_WITH_API_CALLS
                Console.WriteLine("Uncached API Call: {0}", uri.AbsoluteUri);
#endif
                    try
                    {
                        result = JSON.FromUri(uri, UserAgent, AccessToken, Timeout);
                    }
                    catch (WebException ex)
                    {
                        try
                        {
                            using (var stream = ex.Response.GetResponseStream())
                            {
                                throw ParseException(stream);
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
