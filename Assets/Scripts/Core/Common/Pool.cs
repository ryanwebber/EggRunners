using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Pool : MonoBehaviour
{
    private static Dictionary<PoolObject, Pool> instances;
    static Pool() {
        instances = new Dictionary<PoolObject, Pool>();
    }

    [SerializeField]
    private PoolObject prefab;

    [SerializeField]
    private bool isGlobal;

    private Stack<PoolObject> pool;

    private void Awake()
    {
        pool = new Stack<PoolObject>();
        if (isGlobal && prefab != null)
        {
            Assert.IsFalse(instances.ContainsKey(prefab));
            instances[prefab] = this;
        }
    }

    private void OnDestroy()
    {
        if (isGlobal && prefab != null)
        {
            Assert.IsTrue(instances.ContainsKey(prefab));
            instances.Remove(prefab);
        }
    }

    public void ReturnToPool<T>(T obj) where T: Component
    {
        if (obj.TryGetComponent<PoolObject>(out var pObj))
            ReturnToPool(pObj);
    }

    public void ReturnToPool(PoolObject obj)
    {
        if (!isGlobal && instances.TryGetValue(prefab, out var globalPool))
        {
            globalPool.ReturnToPool(obj);
            return;
        }

        obj.enabled = false;
        pool.Push(obj);
    }

    public T GetFromPool<T>() where T: Component
    {
        if (!isGlobal && instances.TryGetValue(prefab, out var globalPool))
        {
            return globalPool.GetFromPool<T>();
        }

        EnsureAtLeast(1);
        var instance = pool.Pop();
        return instance.GetComponent<T>();
    }

    private void EnsureAtLeast(int n)
    {
        while (pool.Count < n)
        {
            var instance = Instantiate(prefab, transform);
            ReturnToPool(instance);
        }
    }
}
