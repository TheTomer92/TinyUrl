using TinyUrl.Models;

namespace TinyUrl.Utils
{
    /// <summary>
    /// An LRU cache.
    /// </summary>
    public class Cache
    {
        private readonly int _capacity;
        private readonly Dictionary<string, Node<UrlMapping>> _cache;
        private readonly CacheLinkedList<UrlMapping> _orderedMappings;
        private readonly object _lock;

        public Cache(int capacity)
        {
            _capacity = capacity;
            _cache = new Dictionary<string, Node<UrlMapping>>(capacity);
            _orderedMappings = new CacheLinkedList<UrlMapping>();
            _lock = new object();
        }

        public UrlMapping? Get(string key)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var node))
                {
                    _orderedMappings.MoveToFront(node);
                    return node.Value;
                }
                return null;
            }
        }

        public void Put(string key, UrlMapping value)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var node))
                {
                    node.Value = value;
                    _orderedMappings.MoveToFront(node);
                }
                else
                {
                    if (_cache.Count >= _capacity)
                    {
                        var lastNode = _orderedMappings.RemoveLast();
                        if (lastNode != null)
                        {
                            _cache.Remove(lastNode.Value.ShortUrl);
                        }
                    }
                    var newNode = new Node<UrlMapping> { Value = value };
                    _orderedMappings.AddFirst(newNode);
                    _cache[key] = newNode;
                }
            }
        }
    }
}
