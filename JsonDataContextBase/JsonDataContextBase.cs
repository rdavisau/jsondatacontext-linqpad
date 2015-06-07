using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace JsonDataContext
{
    public class JsonDataContextBase
    {
        protected static IEnumerable<T> DeserializeSequenceFromJsonFile<T>(string filePath)
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
    }
}