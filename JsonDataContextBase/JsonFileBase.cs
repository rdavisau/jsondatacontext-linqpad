using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonDataContext
{
    public class JsonFileBase<T>
    {
        private static List<T> _cachedData;
        public static bool ShouldCacheData;

        public static void InvalidateCache()
        {
            _cachedData = null;
        }

    }
}
