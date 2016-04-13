using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicsCrm.WebsiteIntegration.Core
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TValue>(this IDictionary<string, TValue> dict, string key,  TValue defaultValue)
        {
            TValue value = defaultValue;
            if (dict.TryGetValue(key, out value))
            {
                return value;
            }
            return defaultValue;
        }
    }
}
