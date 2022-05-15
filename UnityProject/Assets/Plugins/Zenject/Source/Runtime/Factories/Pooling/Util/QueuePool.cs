using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    public class QueuePool<T> : StaticMemoryPool<Queue<T>>
    {
        static QueuePool<T> _instance = new QueuePool<T>();

        public QueuePool()
        {
            OnSpawnMethod = OnSpawned;
            OnDespawnedMethod = OnDespawned;
        }

        public static QueuePool<T> Instance
        {
            get { return _instance; }
        }

        static void OnSpawned(Queue<T> items)
        {
            Assert.That(items.IsEmpty());
        }

        static void OnDespawned(Queue<T> items)
        {
            items.Clear();
        }
    }
}