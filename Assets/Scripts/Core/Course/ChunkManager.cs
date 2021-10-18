using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ChunkManager : MonoBehaviour
{
    [SerializeField]
    private Transform reference;

    [SerializeField]
    private Transform targetParent;

    [SerializeField]
    private Vector3 origin;

    [SerializeField]
    private List<CourseChunk> tailChunks;

    private List<CourseChunk> chunks;

    private void Awake()
    {
        if (chunks == null)
            chunks = new List<CourseChunk>();
    }

    public void SetChunkPrefabs(IList<CourseChunk> newChunks)
    {
        Assert.AreEqual(chunks.Count, 0);

        float accumulatedDistance = 0;
        if (newChunks.Count > 0)
        {
            // Position the first chunk so it starts exactly at the
            // offset position
            accumulatedDistance -= newChunks[0].Length / 2;
        }

        int i = 0;
        foreach (var chunk in newChunks)
        {
            var instance = Instantiate(chunk, targetParent);
            instance.name = $"Course Chunk { i++ }";

            var lengthInWorldUnits = chunk.Length;
            instance.transform.position = origin + Vector3.forward * (accumulatedDistance + lengthInWorldUnits);
            accumulatedDistance += lengthInWorldUnits;

            chunks.Add(instance);
        }

        // Finish line and stuff
        foreach (var chunk in tailChunks)
        {
            var lengthInWorldUnits = chunk.Length;
            chunk.transform.position = origin + Vector3.forward * (accumulatedDistance + lengthInWorldUnits);
            accumulatedDistance += lengthInWorldUnits;

            chunks.Add(chunk);
        }

        tailChunks.Clear();
    }
}
