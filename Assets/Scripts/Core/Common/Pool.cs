using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{
    [SerializeField]
    private PoolObject prefab;

    [SerializeField, Min(0)]
    private int initialCount;

    private Stack<PoolObject> pool;

    private void Awake()
    {
        EnsureAtLeast(initialCount);
    }

    public void ReturnToPool(PoolObject obj)
    {
        obj.enabled = false;
        pool.Push(obj);
    }

    public T GetFromPool<T>() where T: Component
    {
        EnsureAtLeast(1);
        var instance = pool.Pop();
        return instance.GetComponent<T>();
    }

    private void EnsureAtLeast(int n)
    {
        while (pool.Count < n)
        {
            var instance = Instantiate(prefab);
            ReturnToPool(instance);
        }
    }
}
