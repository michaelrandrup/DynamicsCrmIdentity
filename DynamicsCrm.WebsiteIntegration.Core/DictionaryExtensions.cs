using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DynamicsCrm.WebsiteIntegration.Core
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TValue>(this IDictionary<string, TValue> dict, string key, TValue defaultValue, bool removeAfterGet = false)
        {
            TValue value = defaultValue;
            if (dict.TryGetValue(key, out value))
            {
                if (removeAfterGet)
                {
                    dict.Remove(key);
                }
                return value;
            }
            return defaultValue;
        }

        public static bool TryGetArrayValue<TValue>(this IDictionary<string, TValue> dict, string key, int index, out TValue value)
        {
            string keyBase = string.Format("{0}[{1}]", key, index);
            if (!dict.Any(kv => kv.Key.Equals(keyBase, StringComparison.OrdinalIgnoreCase)))
            {
                value = default(TValue);
                return false;
            }
            else
            {
                value = dict[keyBase];
                return true;
            }
        }

        public static void ResolveArrays<TValue>(this IDictionary<string, TValue> dict, string separator = ",")
        {
            while (dict.Any(kv => Regex.IsMatch(kv.Key, "\\[[0-9]\\]")))
            {
                KeyValuePair<string, TValue> kv = dict.First(x => Regex.IsMatch(x.Key, "\\[[0-9]\\]"));
                string keyBase = kv.Key.Substring(0, kv.Key.IndexOf('['));
                string valueBase = Convert.ToString(dict[kv.Key]);
                dict.Remove(kv.Key);
                while (dict.Any(x => Regex.IsMatch(x.Key, string.Format("{0}\\[[0-9]\\]",keyBase))))
                {
                    kv = dict.First(x => Regex.IsMatch(x.Key, string.Format("{0}\\[[0-9]\\]", keyBase)));
                    valueBase += separator + Convert.ToString(dict[kv.Key]);
                    dict.Remove(kv.Key);
                }
                dict.Add(keyBase, (TValue)Convert.ChangeType(valueBase, typeof(TValue)));
            }
        }
    }
}
