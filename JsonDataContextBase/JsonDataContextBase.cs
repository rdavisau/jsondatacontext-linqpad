using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using Newtonsoft.Json;


namespace JsonDataContext
{
    public class JsonDataContextBase
    {
        public Dictionary<string, string> _jsonTextInputs = new Dictionary<string, string>(); 

        protected IEnumerable<T> GetFileJsonInput<T>(string filePath)
        {
            var stream = File.OpenRead(filePath);

            return ReadFromJsonStream<T>(stream);
        }

        protected IEnumerable<T> GetTextJsonInput<T>(string key)
        {
            var json = "";

            if (!_jsonTextInputs.TryGetValue(key, out json))
                throw new Exception(String.Format("Could not find json data for key '{0}'", key));

            if (!json.Trim().StartsWith("["))
                json = String.Format("[{0}]", json.Trim());

            var stream = ToStream(json);

            return ReadFromJsonStream<T>(stream);
        }

        protected IEnumerable<T> GetUrlParameterlessInput<T>(string url, List<Tuple<string,string>> headers)
        {
            var req = (HttpWebRequest) HttpWebRequest.Create(url);
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            foreach (var h in headers)
            {
                var name = h.Item1;
                var val = h.Item2;

                switch (name.ToLower())
                {
                    case "accept":
                        req.Accept = val;
                        break;
                    case "user-agent":
                        req.UserAgent = val;
                        break;
                    default:
                        req.Headers.Add(String.Format("{0}:{1}", name, val));
                        break;
                }
            }

            _globalWebRequestIntercept?.Invoke(req);
            GetSpecificWebRequestIntercept<T>()?.Invoke(req);

            var stream = req.GetResponse().GetResponseStream();

            return ReadFromJsonStream<T>(stream);
        }

        private static IEnumerable<T> ReadFromJsonStream<T>(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                using (var reader = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer();

                    if (!reader.Read())
                        throw new InvalidDataException("Could not interpret input as JSON");

                    // not an array
                    if (reader.TokenType != JsonToken.StartArray)
                    {
                        var item = serializer.Deserialize<T>(reader);
                        yield return item;
                        yield break;
                    }

                    // yes an array
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.EndArray) break;
                        var item = serializer.Deserialize<T>(reader);
                        yield return item;
                    }
                }
            }
        }

        protected static Stream ToStream(string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public void SetGlobalWebRequestIntercept(Action<HttpWebRequest> intercept)
        {
            _globalWebRequestIntercept = intercept;
        }

        public void RemoveGlobalWebRequestIntercept()
        {
            _globalWebRequestIntercept = request => { };
        }

        public Action<HttpWebRequest> GetSpecificWebRequestIntercept<T>()
        {
            Action<HttpWebRequest> intercept = null;
            _webRequestIntercepts.TryGetValue(typeof (T), out intercept);

            return intercept;
        }

        public void SetSpecificWebRequestIntercept<T>(Action<HttpWebRequest> intercept)
        {
            _webRequestIntercepts[typeof(T)] = intercept;
        }

        public void RemoveSpecificWebRequestIntercept<T>()
        {
            if (_webRequestIntercepts.ContainsKey(typeof (T)))
                _webRequestIntercepts.Remove(typeof (T));
        }
        public void RemoveAllSpecificWebRequestIntercepts<T>()
        {
            _webRequestIntercepts.Clear();
        }

        public List<KeyValuePair<Type, Action<HttpWebRequest>>> GetSpecificWebRequestIntercepts()
        {
            return _webRequestIntercepts.ToList();
        }

        private Action<HttpWebRequest> _globalWebRequestIntercept = request => { };
        private readonly Dictionary<Type,Action<HttpWebRequest>> _webRequestIntercepts = new Dictionary<Type, Action<HttpWebRequest>>();
    }
}