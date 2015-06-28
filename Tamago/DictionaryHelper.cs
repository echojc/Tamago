using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tamago
{
    public static class DictionaryHelper
    {
        /// <summary>
        /// Returns the value for the given key if it exists, otherwise the default value.
        /// 
        /// Useful for converting dictionaries to a function with the behaviour required by expressions.
        /// </summary>
        /// <param name="dict">The dictionary to look up.</param>
        /// <param name="key">The key to look up.</param>
        /// <returns></returns>
        public static V GetValueOrDefault<K, V>(this Dictionary<K, V> dict, K key)
        {
            V value;
            dict.TryGetValue(key, out value);
            return value;
        }
    }
}
