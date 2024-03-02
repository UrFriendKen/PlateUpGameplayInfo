using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KitchenGameplayInfo.Utils
{
    internal static class ResourceUtils
    {
        private abstract class ResourceCollection
        {
            public readonly Type Type;
            protected Dictionary<string, UnityEngine.Object> _resources;

            public ResourceCollection(Type type)
            {
                Type = type;
                _resources = new Dictionary<string, UnityEngine.Object>();
            }

            public bool ContainsKey(string key) => _resources.ContainsKey(key);

            protected void Add(string key, UnityEngine.Object obj) => _resources.Add(key, obj);
        }

        private class ResourceCollection<T> : ResourceCollection where T : UnityEngine.Object
        {
            public Dictionary<string, T> Items => _resources.ToDictionary(x => x.Key, x => (T)x.Value);

            public T this[string key]
            {
                get
                {
                    return (T)_resources[key];
                }
            }

            public void Add(string key, T obj) => base.Add(key, obj);

            public ResourceCollection() : base(typeof(T))
            {
            }
        }

        static Dictionary<Type, ResourceCollection> _resources = new Dictionary<Type, ResourceCollection>();

        internal static void Init()
        {
            InitCollection<Material>();
        }

        private static Dictionary<string, T> InitCollection<T>(bool reinit = false) where T : UnityEngine.Object
        {
            Type type = typeof(T);
            if (reinit && _resources.ContainsKey(type))
                _resources.Remove(type);

            if (!_resources.TryGetValue(type, out ResourceCollection resourceCollection) || !(resourceCollection is ResourceCollection<T> resourceCollectionTyped))
            {
                resourceCollectionTyped = new ResourceCollection<T>();
                foreach (T item in Resources.FindObjectsOfTypeAll(type) as T[])
                {
                    if (!resourceCollectionTyped.ContainsKey(item.name))
                    {
                        resourceCollectionTyped.Add(item.name, item);
                    }
                }
                _resources.Add(type, resourceCollectionTyped);
            }
            return resourceCollectionTyped.Items;
        }

        public static Dictionary<string, T> Get<T>() where T : UnityEngine.Object
        {
            return InitCollection<T>();
        }

        public static T Get<T>(string resourceName) where T : UnityEngine.Object
        {
            TryGet(resourceName, out T resource);
            return resource;
        }

        public static bool TryGet<T>(string resourceName, out T resource, bool warn_if_fail = false) where T : UnityEngine.Object
        {
            Dictionary<string, T> resourceCollection = InitCollection<T>();
            if (resourceCollection.TryGetValue(resourceName, out T value))
            {
                resource = value;
                return true;
            }
            if (warn_if_fail)
                Main.LogError($"Failed to get {typeof(T)} with name \"{resourceName}\"");
            resource = null;
            return false;
        }
    }
}
