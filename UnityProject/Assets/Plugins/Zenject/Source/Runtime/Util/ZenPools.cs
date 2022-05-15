using System;
using System.Collections.Generic;

namespace Zenject.Internal
{
    internal static class ZenPools
    {
#if ZEN_INTERNAL_NO_POOLS
        public static InjectContext SpawnInjectContext(DiContainer container, Type memberType, DisposeBlock disposeBlock = null)
        {
            return new InjectContext(container, memberType);
        }

        public static void DespawnInjectContext(InjectContext context)
        {
        }

        public static List<T> SpawnList<T>(DisposeBlock disposeBlock = null)
        {
            return new List<T>();
        }

        public static void DespawnList<T>(List<T> list)
        {
        }

        public static void DespawnArray<T>(T[] arr)
        {
        }

        public static T[] SpawnArray<T>(int length, DisposeBlock disposeBlock = null)
        {
            return new T[length];
        }

        public static HashSet<T> SpawnHashSet<T>(DisposeBlock disposeBlock = null)
        {
            return new HashSet<T>();
        }

        public static Dictionary<TKey, TValue> SpawnDictionary<TKey, TValue>(DisposeBlock disposeBlock = null)
        {
            return new Dictionary<TKey, TValue>();
        }

        public static void DespawnDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
        }

        public static void DespawnHashSet<T>(HashSet<T> set)
        {
        }

        public static LookupId SpawnLookupId(IProvider provider, BindingId bindingId, DisposeBlock disposeBlock = null)
        {
            return new LookupId(provider, bindingId);
        }

        public static void DespawnLookupId(LookupId lookupId)
        {
        }

        public static BindInfo SpawnBindInfo(DisposeBlock disposeBlock = null)
        {
            return new BindInfo();
        }

        public static void DespawnBindInfo(BindInfo bindInfo)
        {
        }

        public static BindStatement SpawnStatement(DisposeBlock disposeBlock = null)
        {
            return new BindStatement();
        }

        public static void DespawnStatement(BindStatement statement)
        {
        }
        
        public static Queue<T> SpawnQueue<T>(DisposeBlock disposeBlock = null)
        {
            return new Queue<T>();
        }

        public static void DespawnQueue<T>(Queue<T> queue)
        {
        }
#else
        static readonly StaticMemoryPool<InjectContext> _contextPool = new StaticMemoryPool<InjectContext>();
        static readonly StaticMemoryPool<LookupId> _lookupIdPool = new StaticMemoryPool<LookupId>();
        static readonly StaticMemoryPool<BindInfo> _bindInfoPool = new StaticMemoryPool<BindInfo>();
        static readonly StaticMemoryPool<BindStatement> _bindStatementPool = new StaticMemoryPool<BindStatement>();

        public static HashSet<T> SpawnHashSet<T>(DisposeBlock disposeBlock = null)
        {
            return disposeBlock?.Spawn(HashSetPool<T>.Instance) ?? HashSetPool<T>.Instance.Spawn();
        }

        public static Dictionary<TKey, TValue> SpawnDictionary<TKey, TValue>(DisposeBlock disposeBlock = null)
        {
            return disposeBlock?.Spawn(DictionaryPool<TKey, TValue>.Instance) ?? DictionaryPool<TKey, TValue>.Instance.Spawn();
        }

        public static BindStatement SpawnStatement(DisposeBlock disposeBlock = null)
        {
            return disposeBlock?.Spawn(_bindStatementPool) ?? _bindStatementPool.Spawn();
        }

        public static void DespawnStatement(BindStatement statement)
        {
            _bindStatementPool.Despawn(statement);
        }

        public static BindInfo SpawnBindInfo(DisposeBlock disposeBlock = null)
        {
            return disposeBlock?.Spawn(_bindInfoPool) ?? _bindInfoPool.Spawn(); 
        }

        public static void DespawnBindInfo(BindInfo bindInfo)
        {
            _bindInfoPool.Despawn(bindInfo);
        }

        public static void DespawnDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            DictionaryPool<TKey, TValue>.Instance.Despawn(dictionary);
        }

        public static void DespawnHashSet<T>(HashSet<T> set)
        {
            HashSetPool<T>.Instance.Despawn(set);
        }

        public static LookupId SpawnLookupId(IProvider provider, BindingId bindingId, DisposeBlock disposeBlock = null)
        {
            var lookupId = disposeBlock?.Spawn(_lookupIdPool) ?? _lookupIdPool.Spawn();

            lookupId.Provider = provider;
            lookupId.BindingId = bindingId;

            return lookupId;
        }

        public static void DespawnLookupId(LookupId lookupId)
        {
            _lookupIdPool.Despawn(lookupId);
        }

        public static List<T> SpawnList<T>(DisposeBlock disposeBlock = null)
        {
            return disposeBlock?.Spawn(ListPool<T>.Instance) ?? ListPool<T>.Instance.Spawn();
        }

        public static void DespawnList<T>(List<T> list)
        {
            ListPool<T>.Instance.Despawn(list);
        }

        public static void DespawnArray<T>(T[] arr)
        {
            ArrayPool<T>.GetPool(arr.Length).Despawn(arr);
        }

        public static T[] SpawnArray<T>(int length, DisposeBlock disposeBlock = null)
        {
            ArrayPool<T> pool = ArrayPool<T>.GetPool(length);
            return disposeBlock?.Spawn(pool) ?? pool.Spawn();
        }

        public static InjectContext SpawnInjectContext(DiContainer container, Type memberType, DisposeBlock disposeBlock = null)
        {
            var context = disposeBlock?.Spawn(_contextPool) ?? _contextPool.Spawn();

            context.Container = container;
            context.MemberType = memberType;

            return context;
        }

        public static void DespawnInjectContext(InjectContext context)
        {
            _contextPool.Despawn(context);
        }

        public static Queue<T> SpawnQueue<T>(DisposeBlock disposeBlock = null)
        {
            return disposeBlock?.Spawn(QueuePool<T>.Instance) ?? QueuePool<T>.Instance.Spawn();
        }

        public static void DespawnQueue<T>(Queue<T> queue)
        {
            QueuePool<T>.Instance.Despawn(queue);
        }
#endif

        public static InjectContext SpawnInjectContext(
            DiContainer container, InjectableInfo injectableInfo, InjectContext currentContext,
            object targetInstance, Type targetType, object concreteIdentifier)
        {
            var context = SpawnInjectContext(container, injectableInfo.MemberType);

            context.ObjectType = targetType;
            context.ParentContext = currentContext;
            context.ObjectInstance = targetInstance;
            context.Identifier = injectableInfo.Identifier;
            context.MemberName = injectableInfo.MemberName;
            context.Optional = injectableInfo.Optional;
            context.SourceType = injectableInfo.SourceType;
            context.FallBackValue = injectableInfo.DefaultValue;
            context.ConcreteIdentifier = concreteIdentifier;

            return context;
        }
    }
}
