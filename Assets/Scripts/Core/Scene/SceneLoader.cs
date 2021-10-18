using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface ISceneLoader
{
    public T GetContext<T>();
    public bool TryGetContext<T>(out T context);

    public ILimboObject RecoverGameObject(SceneStoreKey key);
    public bool TryRecoverGameObject(SceneStoreKey key, out ILimboObject gameObject);
}

public class SceneLoader: ISceneLoader
{
    private Scene targetScene;
    private Dictionary<Type, object> contextStore;
    private Dictionary<SceneStoreKey, GameObject> gameObjectStore;

    public SceneLoader(Scene targetScene, Dictionary<Type, object> contextStore, Dictionary<SceneStoreKey, GameObject> gameObjectStore)
    {
        this.targetScene = targetScene;
        this.contextStore = contextStore;
        this.gameObjectStore = gameObjectStore;
    }

    public T GetContext<T>()
    {
        if (contextStore.TryGetValue(typeof(T), out var value) && value is T instance)
            return instance;
        else
            throw new Exception($"No stored context: {typeof(T)}");
    }

    public bool TryGetContext<T>(out T context)
    {
        if (contextStore.TryGetValue(typeof(T), out var value) && value is T instance)
        {
            context = instance;
            return true;
        }

        context = default;
        return false;
    }

    public ILimboObject RecoverGameObject(SceneStoreKey key)
    {
        if (gameObjectStore.TryGetValue(key, out var foundObject))
        {
            return new LimboObject
            {
                gameObject = foundObject,
                reparent = (transform) =>
                {
                    SceneManager.MoveGameObjectToScene(foundObject, targetScene);
                    foundObject.transform.parent = transform;
                }
            };
        }

        throw new Exception($"No stored game object stored with key: {key}");
    }

    public bool TryRecoverGameObject(SceneStoreKey key, out ILimboObject gameObject)
    {
        if (gameObjectStore.TryGetValue(key, out var foundObject))
        {
            gameObject = new LimboObject
            {
                gameObject = foundObject,
                reparent = (transform) =>
                {
                    SceneManager.MoveGameObjectToScene(foundObject, targetScene);
                    foundObject.transform.parent = transform;
                }
            };

            return true;
        }

        gameObject = default;
        return false;
    }
}
