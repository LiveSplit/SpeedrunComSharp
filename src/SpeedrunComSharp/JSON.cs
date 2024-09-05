using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;

namespace SpeedrunComSharp;

internal static class JSON
{
    public static dynamic FromResponse(WebResponse response)
    {
        using Stream stream = response.GetResponseStream();
        return FromStream(stream);
    }

    public static dynamic FromStream(Stream stream)
    {
        var reader = new StreamReader(stream);
        string json = "";
        try
        {
            json = reader.ReadToEnd();
        }
        catch { }

        return FromString(json);
    }

    public static dynamic FromString(string value)
    {
        var serializer = new JavaScriptSerializer()
        {
            MaxJsonLength = int.MaxValue
        };
        serializer.RegisterConverters(new[] { new DynamicJsonConverter() });

        return serializer.Deserialize<object>(value);
    }

    public static dynamic FromUri(Uri uri, string userAgent, string accessToken, TimeSpan timeout)
    {
        var request = (HttpWebRequest)WebRequest.Create(uri);
        request.Timeout = (int)timeout.TotalMilliseconds;
        request.UserAgent = userAgent;
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Add("X-API-Key", accessToken.ToString());
        }

        WebResponse response = request.GetResponse();
        return FromResponse(response);
    }

    public static string Escape(string value)
    {
        return HttpUtility.JavaScriptStringEncode(value);
    }

    public static dynamic FromUriPost(Uri uri, string userAgent, string accessToken, TimeSpan timeout, string postBody)
    {
        var request = (HttpWebRequest)WebRequest.Create(uri);
        request.Timeout = (int)timeout.TotalMilliseconds;
        request.Method = "POST";
        request.UserAgent = userAgent;
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Add("X-API-Key", accessToken.ToString());
        }

        request.ContentType = "application/json";

        using (var writer = new StreamWriter(request.GetRequestStream()))
        {
            writer.Write(postBody);
        }

        WebResponse response = request.GetResponse();

        return FromResponse(response);
    }
}

public sealed class DynamicJsonConverter : JavaScriptConverter
{
    public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException("dictionary");
        }

        return type == typeof(object) ? new DynamicJsonObject(dictionary) : null;
    }

    public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<Type> SupportedTypes => new ReadOnlyCollection<Type>(new List<Type>(new[] { typeof(object) }));
}

public sealed class DynamicJsonObject : DynamicObject
{
    private readonly IDictionary<string, object> _dictionary;

    //public IDictionary<string, object> Properties { get { return _dictionary; } }

    public DynamicJsonObject()
        : this(new Dictionary<string, object>())
    { }

    public DynamicJsonObject(IDictionary<string, object> dictionary)
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException("dictionary");
        }

        _dictionary = dictionary;
    }

    public override string ToString()
    {
        var sb = new StringBuilder("{\r\n");
        ToString(sb);
        return sb.ToString();
    }

    private void ToString(StringBuilder sb, int depth = 1)
    {
        bool firstInDictionary = true;
        foreach (KeyValuePair<string, object> pair in _dictionary)
        {
            if (!firstInDictionary)
            {
                sb.Append(",\r\n");
            }

            sb.Append('\t', depth);
            firstInDictionary = false;
            object value = pair.Value;
            string name = pair.Key;
            if (value is IEnumerable<object> array)
            {
                sb.Append("\"" + HttpUtility.JavaScriptStringEncode(name) + "\": [\r\n");
                bool firstInArray = true;
                foreach (object arrayValue in array)
                {
                    if (!firstInArray)
                    {
                        sb.Append(",\r\n");
                    }

                    sb.Append('\t', depth + 1);
                    firstInArray = false;
                    if (arrayValue is IDictionary<string, object> dict)
                    {
                        new DynamicJsonObject(dict).ToString(sb, depth + 2);
                    }
                    else if (arrayValue is DynamicJsonObject obj)
                    {
                        sb.Append("{\r\n");
                        obj.ToString(sb, depth + 2);
                    }
                    else if (arrayValue is string str)
                    {
                        sb.AppendFormat("\"{0}\"", HttpUtility.JavaScriptStringEncode(str));
                    }
                    else if (arrayValue is bool b)
                    {
                        sb.AppendFormat("{0}", HttpUtility.JavaScriptStringEncode(b.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
                    }
                    else if (arrayValue is int i)
                    {
                        sb.AppendFormat("{0}", HttpUtility.JavaScriptStringEncode(i.ToString(CultureInfo.InvariantCulture)));
                    }
                    else if (arrayValue is double d)
                    {
                        sb.AppendFormat("{0}", HttpUtility.JavaScriptStringEncode(d.ToString(CultureInfo.InvariantCulture)));
                    }
                    else if (arrayValue is decimal m)
                    {
                        sb.AppendFormat("{0}", HttpUtility.JavaScriptStringEncode(m.ToString(CultureInfo.InvariantCulture)));
                    }
                    else
                    {
                        sb.AppendFormat("\"{0}\"", HttpUtility.JavaScriptStringEncode((arrayValue ?? "").ToString()));
                    }
                }

                sb.Append("\r\n");
                sb.Append('\t', depth);
                sb.Append("]");
            }
            else if (value is IDictionary<string, object> dict)
            {
                sb.Append("\"" + HttpUtility.JavaScriptStringEncode(name) + "\": {\r\n");
                new DynamicJsonObject(dict).ToString(sb, depth + 1);
            }
            else if (value is DynamicJsonObject obj)
            {
                sb.Append("\"" + HttpUtility.JavaScriptStringEncode(name) + "\": {\r\n");
                obj.ToString(sb, depth + 1);
            }
            else if (value is string str)
            {
                sb.AppendFormat("\"{0}\": \"{1}\"", HttpUtility.JavaScriptStringEncode(name), HttpUtility.JavaScriptStringEncode(str));
            }
            else if (value is bool b)
            {
                sb.AppendFormat("\"{0}\": {1}", HttpUtility.JavaScriptStringEncode(name), HttpUtility.JavaScriptStringEncode(b.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            else if (value is int i)
            {
                sb.AppendFormat("\"{0}\": {1}", HttpUtility.JavaScriptStringEncode(name), HttpUtility.JavaScriptStringEncode(i.ToString(CultureInfo.InvariantCulture)));
            }
            else if (value is double d)
            {
                sb.AppendFormat("\"{0}\": {1}", HttpUtility.JavaScriptStringEncode(name), HttpUtility.JavaScriptStringEncode(d.ToString(CultureInfo.InvariantCulture)));
            }
            else if (value is decimal m)
            {
                sb.AppendFormat("\"{0}\": {1}", HttpUtility.JavaScriptStringEncode(name), HttpUtility.JavaScriptStringEncode(m.ToString(CultureInfo.InvariantCulture)));
            }
            else
            {
                sb.AppendFormat("\"{0}\": \"{1}\"", HttpUtility.JavaScriptStringEncode(name), HttpUtility.JavaScriptStringEncode((value ?? "").ToString()));
            }
        }

        sb.Append("\r\n");
        sb.Append('\t', depth - 1);
        sb.Append("}");
    }

    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        if (_dictionary.ContainsKey(binder.Name))
        {
            _dictionary[binder.Name] = value;
            return true;
        }
        else
        {
            _dictionary.Add(binder.Name, value);
            return true;
        }
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        if (binder.Name == "Properties")
        {
            result = _dictionary
                .Select(x => new KeyValuePair<string, dynamic>(x.Key, WrapResultObject(x.Value)))
                .ToDictionary(x => x.Key, x => x.Value);
            return true;
        }

        if (!_dictionary.TryGetValue(binder.Name, out result))
        {
            // return null to avoid exception.  caller can check for null this way...
            result = null;
            return true;
        }

        result = WrapResultObject(result);

        if (result is string)
        {
            result = JavaScriptStringDecode(result as string);
        }

        return true;
    }

    public static string JavaScriptStringDecode(string source)
    {
        // Replace some chars.
        string decoded = source.Replace(@"\'", "'")
                    .Replace(@"\""", @"""")
                    .Replace(@"\/", "/")
                    .Replace(@"\t", "\t")
                    .Replace(@"\n", "\n");

        // Replace unicode escaped text.
        var rx = new Regex(@"\\[uU]([0-9A-F]{4})");

        decoded = rx.Replace(decoded, match => ((char)int.Parse(match.Value[2..], NumberStyles.HexNumber))
                                                .ToString(CultureInfo.InvariantCulture));

        return decoded;
    }

    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
    {
        if (indexes.Length == 1 && indexes[0] != null)
        {
            if (!_dictionary.TryGetValue(indexes[0].ToString(), out result))
            {
                // return null to avoid exception.  caller can check for null this way...
                result = null;
                return true;
            }

            result = WrapResultObject(result);
            return true;
        }

        return base.TryGetIndex(binder, indexes, out result);
    }

    private static object WrapResultObject(object result)
    {
        if (result is IDictionary<string, object> dictionary)
        {
            return new DynamicJsonObject(dictionary);
        }

        if (result is ArrayList arrayList && arrayList.Count > 0)
        {
            return arrayList[0] is IDictionary<string, object>
                ? new List<object>(arrayList.Cast<IDictionary<string, object>>().Select(x => new DynamicJsonObject(x)))
                : new List<object>(arrayList.Cast<object>());
        }

        return result;
    }
}
