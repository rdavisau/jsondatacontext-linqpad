using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace JsonDataContext
{
    public class JsonDataContextBase<TLocation>
    {
        protected Dictionary<TLocation, string> LocationPaths;

        public void Save<T>(IEnumerable<T> items, TLocation location, JsonSerializerSettings serializationSettings = null)
        {
            var loc = LocationPaths[location];
            SaveToCustomLocation(items, loc, serializationSettings);
        }

        public void SaveToCustomLocation<T>(IEnumerable<T> items, string location, JsonSerializerSettings serializationSettings = null)
        {
            var json = JsonConvert.SerializeObject(items, serializationSettings ?? new JsonSerializerSettings());
            File.WriteAllText(location, json);
        }
    }
}