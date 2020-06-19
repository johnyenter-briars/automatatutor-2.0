using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.Utils
{
    public class MultiDictionary<K, V>
    {
        private Dictionary<K, List<V>> dictionary = new Dictionary<K, List<V>>();

        public MultiDictionary()
        {
        }

        public void Add(K key, V value)
        {
            if (dictionary.TryGetValue(key, out List<V> list))
            {
                list.Add(value);
            }
            else
            {
                dictionary[key] = new List<V>()
                {
                    value
                };
            }
        }

        public IEnumerable<V> ValuesOfKey(K key)
        {
            if (dictionary.TryGetValue(key, out List<V> values))
            {
                return values;
            }
            return new List<V>();
        }

        public IEnumerable<V> Values
        {
            get
            {
                return dictionary.Values.SelectMany(v => v);
            }
        }
    }
}
