using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShredderComponent : MonoBehaviour
{
    [System.Serializable]
    private struct VelocityRange
    {
        [SerializeField]
        private Vector3 a;

        [SerializeField]
        private Vector3 b;

        public Vector3 RandomVelocity() => Vector3.Lerp(a, b, Random.value);
    }

    [SerializeField]
    private Pool spawnPool;

    [SerializeField]
    private float spawnInterval;

    [SerializeField]
    private VelocityRange spawnVelocity;

    [SerializeField]
    private SpawnBoundary boundary;

    [SerializeField]
    private DynamicCourseComponent dynamicController;

    private List<PoolObject> aliveObjects;
    private Coroutine spawnerRoutine;

    private void Awake()
    {
        aliveObjects = new List<PoolObject>();

        dynamicController.OnDynamicComponentStart += () =>
        {
            if (spawnerRoutine == null)
            {
                spawnerRoutine = StartCoroutine(StartSpawning());
            }
        };

        dynamicController.OnDynamicComponentReset += () =>
        {
            if (spawnerRoutine != null)
            {
                StopCoroutine(spawnerRoutine);
            }

            spawnerRoutine = null;

            for (int i = aliveObjects.Count - 1; i >= 0; i--)
                aliveObjects[i].ReturnToPool();

            aliveObjects.Clear();
        };
    }

    private void Update()
    {
        for (int i = aliveObjects.Count - 1; i >= 0; i--)
        {
            if (aliveObjects[i].transform.position.y < -64f)
            {
                aliveObjects[i].ReturnToPool();
                aliveObjects.RemoveAt(i);
            }
        }
    }

    private IEnumerator StartSpawning()
    {
        while (true)
        {
            var instance = spawnPool.GetFromPool<PoolObject>();
            var spawnBounds = boundary.WorldBounds;
            instance.transform.position = new Vector3(
                Random.Range(spawnBounds.min.x, spawnBounds.max.x),
                Random.Range(spawnBounds.min.y, spawnBounds.max.y),
                Random.Range(spawnBounds.min.z, spawnBounds.max.z));

            Debug.Log($"Spawning cannonball at {instance.transform.position}", instance);

            if (instance.TryGetComponent<Rigidbody>(out var rigidbody))
            {
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.velocity = spawnVelocity.RandomVelocity();
            }

            aliveObjects.Add(instance);

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
