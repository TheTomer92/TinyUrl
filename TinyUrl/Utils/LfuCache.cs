namespace TinyUrl.Utils
{
    public class CacheItem<T>
    {
        public required string Key { get; set; }
        public required T Value { get; set; }
        public int Frequency { get; set; }
    }

    public class LFUCache<TValue>
    {
        private readonly int _cashSize;
        private readonly Dictionary<string, CacheItem<TValue>> _cache;
        private readonly SortedDictionary<int, LinkedList<CacheItem<TValue>>> _frequencyList;
        private readonly object _lock;

        public LFUCache(int cashSize)
        {
            _cashSize = cashSize;
            _cache = [];
            _frequencyList = [];
            _lock = new();
        }

        public TValue? Get(string key)
        {
            lock (_lock)
            {
                if (!_cache.ContainsKey(key))
                {
                    return default;
                }

                var item = _cache[key];
                UpdateFrequency(item);

                return item.Value;
            }
        }

        public void Put(string key, TValue value)
        {
            lock (_lock)
            {
                if (_cache.ContainsKey(key))
                {
                    var item = _cache[key];
                    item.Value = value;
                    UpdateFrequency(item);
                }
                else
                {
                    if (_cache.Count >= _cashSize)
                    {
                        EvictLeastFrequentlyUsed();
                    }

                    var newItem = new CacheItem<TValue>
                    {
                        Key = key,
                        Value = value,
                        Frequency = 1
                    };

                    _cache[key] = newItem;
                    if (!_frequencyList.ContainsKey(1))
                    {
                        _frequencyList[1] = new LinkedList<CacheItem<TValue>>();
                    }

                    _frequencyList[1].AddLast(newItem);
                }
            }
        }

        private void UpdateFrequency(CacheItem<TValue> item)
        {
            var oldFrequency = item.Frequency;
            item.Frequency++;

            _frequencyList[oldFrequency].Remove(item);
            if (_frequencyList[oldFrequency].Count == 0)
            {
                _frequencyList.Remove(oldFrequency);
            }

            if (!_frequencyList.ContainsKey(item.Frequency))
            {
                _frequencyList[item.Frequency] = new LinkedList<CacheItem<TValue>>();
            }

            _frequencyList[item.Frequency].AddLast(item);
        }

        private void EvictLeastFrequentlyUsed()
        {
            var leastFrequency = _frequencyList.First().Key;
            var leastFrequentlyUsedItems = _frequencyList[leastFrequency];
            var itemToEvict = leastFrequentlyUsedItems.First?.Value;

            leastFrequentlyUsedItems.RemoveFirst();
            if (leastFrequentlyUsedItems.Count == 0)
            {
                _frequencyList.Remove(leastFrequency);
            }

            if (itemToEvict != null) _cache.Remove(itemToEvict.Key);
        }
    }
}
