using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SharedState : MonoBehaviour
{
    private struct TypeProvider
    {
        public string tag;
        public System.Func<object> getter;
        public Handle handle;

        public T GetOr<T>(T fallback)
        {
            var obj = getter?.Invoke();
            if (obj != null && obj is T typedValue)
                return typedValue;
            else
                return fallback;
        }
    }

    private class TypeProviderSet
    {
        public List<TypeProvider> providers;

        public TypeProviderSet()
        {
            providers = new List<TypeProvider>();
        }

        public bool TryGetProvider(out TypeProvider provider)
        {
            return TryGetProvider(null, out provider);
        }

        public bool TryGetProvider(string tag, out TypeProvider provider)
        {
            if (tag == null)
            {
                if (providers.Count > 0)
                {
                    provider = providers[0];
                    return true;
                }
            }
            else
            {
                foreach (var p in providers)
                {
                    if (p.tag == tag)
                    {
                        provider = p;
                        return true;
                    }
                }
            }

            provider = default;
            return false;
        }
    }

    private Dictionary<System.Type, TypeProviderSet> providers;

    private void Awake()
    {
        providers = new Dictionary<System.Type, TypeProviderSet>();
    }

    public T GetStateOrDefault<T>(string tag, T fallback)
    {
        if (providers.TryGetValue(typeof(T), out var set))
        {
            if (set.TryGetProvider(tag, out var provider))
            {
                return provider.GetOr<T>(fallback);
            }
        }

        return fallback;
    }

    public T GetStateOrDefault<T>(T fallback)
    {
        if (providers.TryGetValue(typeof(T), out var set))
        {
            if (set.TryGetProvider(out var provider))
            {
                return provider.GetOr<T>(fallback);
            }
        }

        return fallback;
    }

    public Handle BindState<T>(System.Func<T> func)
    {
        return BindState<T>(null, func);
    }

    public Handle BindState<T>(string tag, System.Func<T> func)
    {
        var handle = Handle.CreateUnique();
        var provider = new TypeProvider
        {
            tag = tag,
            getter = () => func,
            handle = handle,
        };

        if (!providers.ContainsKey(typeof(T)))
            providers[typeof(T)] = new TypeProviderSet();

        providers[typeof(T)].providers.Add(provider);

        return handle;
    }

    public bool Unbind(Handle handle)
    {
        foreach (var set in providers.Values)
        {
            for (int i = 0; i < set.providers.Count; i++)
            {
                if (set.providers[i].handle == handle)
                {
                    set.providers.RemoveAt(i);
                    return true;
                }
            }
        }

        return false;
    }
}
