using System;
using UnityEngine;

public interface ILimboObject
{
    public GameObject ReparentToScene(Transform t);
    public T ReparentToScene<T>(Transform t) where T: MonoBehaviour;
}

public struct LimboObject: ILimboObject
{
    public GameObject gameObject;
    public Action<Transform> reparent;

    public GameObject ReparentToScene(Transform t)
    {
        reparent?.Invoke(t);
        return gameObject;
    }

    public T ReparentToScene<T>(Transform t) where T : MonoBehaviour
        => ReparentToScene(t).GetComponent<T>();
}
