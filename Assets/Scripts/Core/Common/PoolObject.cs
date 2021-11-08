using UnityEngine;
using System.Collections;

public class PoolObject : MonoBehaviour
{
    public Pool AssignedPool { get; set; }

    public void ReturnToPool()
    {
        AssignedPool.ReturnToPool(this);
    }
}
