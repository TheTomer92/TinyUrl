# TinyUrl
URL shortening system.


For the size-limited cache I decided to go with an LFU (Least Frequently Used) eviction policy.
It retains the most frequently accessed items, which can be beneficial if certain URLs are accessed far more frequently than others.

The disadvantage is it can suffer from cache pollution if items that were accessed frequently in the past continue to occupy the cache even if they are no longer accessed.
This can reduce the effectiveness of the cache for new items.
