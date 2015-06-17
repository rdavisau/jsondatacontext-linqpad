using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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

        protected IEnumerable<T> GetUrlParameterlessInput<T>(string url)
        {
            var req = (HttpWebRequest) HttpWebRequest.Create(url);
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            req.UserAgent = @"Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";

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
    }
}