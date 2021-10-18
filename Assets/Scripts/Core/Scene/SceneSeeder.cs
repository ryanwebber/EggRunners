using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public interface IDebugSceneSeeder
{
    public void AddContext<T>(T context);
    public SceneStoreKey Initialize<T>(T prefab) where T: Component;
}

public interface IPersistantInstantiator
{
    T Instantiate<T>(T prefab) where T: Component;
}

public class DebugSceneSeeder : IDebugSceneSeeder
{
    private IPersistantInstantiator factory;
    private Dictionary<Type, object> contextStore;
    private Dictionary<SceneStoreKey, GameObject> gameObjectStore;
    private Dictionary<GameObject, SceneStoreKey> reverseGameObjectStore;

    public DebugSceneSeeder(Dictionary<Type, object> contextStore, IPersistantInstantiator factory)
    {
        this.factory = factory;
        this.contextStore = contextStore;
        this.gameObjectStore = new Dictionary<SceneStoreKey, GameObject>();
        this.reverseGameObjectStore = new Dictionary<GameObject, SceneStoreKey>();
    }

    public void AddContext<T>(T context)
    {
        contextStore.Add(typeof(T), context);
    }

    public SceneStoreKey Initialize<T>(T prefab) where T: Component
    {
        var gameObject = factory.Instantiate(prefab).gameObject;
        var newKey = SceneStoreKey.CreateUnique();
        gameObjectStore.Add(newKey, gameObject);
        reverseGameObjectStore.Add(gameObject, newKey);

        return newKey;
    }

    public ISceneLoader UnloadInScene(Scene targetScene)
    {
        return new SceneLoader(targetScene, contextStore, gameObjectStore);
    }
}
