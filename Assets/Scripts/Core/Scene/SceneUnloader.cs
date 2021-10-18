using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public interface ISceneUnloader
{
    public void SetContext<T>(T context);
    public SceneStoreKey StoreGameObject(GameObject gameObject);
}

public class SceneUnloader : ISceneUnloader
{
    private Scene persistantScene;
    private Transform storageParent;
    private Dictionary<Type, object> contextStore;
    private Dictionary<SceneStoreKey, GameObject> gameObjectStore;
    private Dictionary<GameObject, SceneStoreKey> reverseGameObjectStore;

    public SceneUnloader(Scene persistantScene, Dictionary<Type, object> contextStore, Transform storageParent)
    {
        Assert.IsTrue(persistantScene.isLoaded);
        this.persistantScene = persistantScene;
        this.storageParent = storageParent;
        this.contextStore = contextStore;
        this.gameObjectStore = new Dictionary<SceneStoreKey, GameObject>();
        this.reverseGameObjectStore = new Dictionary<GameObject, SceneStoreKey>();
    }

    public void SetContext<T>(T context)
    {
        contextStore[typeof(T)] = context;
    }

    public SceneStoreKey StoreGameObject(GameObject gameObject)
    {
        if (reverseGameObjectStore.TryGetValue(gameObject, out var key))
            return key;

        Assert.IsFalse(gameObject.scene == persistantScene);

        SceneManager.MoveGameObjectToScene(gameObject, persistantScene);
        gameObject.transform.parent = storageParent;

        var newKey = SceneStoreKey.CreateUnique();
        gameObjectStore.Add(key, gameObject);
        reverseGameObjectStore.Add(gameObject, key);

        return newKey;
    }

    public ISceneLoader UnloadInScene(Scene targetScene)
    {
        return new SceneLoader(targetScene, contextStore, gameObjectStore);
    }
}
