using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;


namespace JsonDataContext
{
    public class JsonDataContextBase
    {
        public Dictionary<string, string> _jsonTextInputs = new Dictionary<string, string>(); 

        protected IEnumerable<T> DeserializeSequenceFromJsonFile<T>(string filePath)
        {
            using (var sr = new StreamReader(filePath))
            { 
                using (var reader = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer();
                    
                    if (!reader.Read() || reader.TokenType != JsonToken.StartArray)
                        throw new Exception("The source input did not appear to be an array of objects.");
                      

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.EndArray) break;
                        var item = serializer.Deserialize<T>(reader);
                        yield return item;
                    }
                }
            }
        }

        protected IEnumerable<T> GetTextJsonInput<T>(string key)
        {
            var json = "";

            if (!_jsonTextInputs.TryGetValue(key, out json))
                throw new Exception(String.Format("Could not find json data for key '{0}'", key));

            if (!json.Trim().StartsWith("["))
                json = String.Format("[{0}]", json.Trim());

            using (var sr = new StreamReader(ToStream(json)))
            {
                using (var reader = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer();

                    if (!reader.Read() || reader.TokenType != JsonToken.StartArray)
                        throw new Exception("The source input did not appear to be an array of objects.");

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