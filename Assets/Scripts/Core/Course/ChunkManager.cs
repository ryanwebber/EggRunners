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

        int i = 0;
        foreach (var chunk in newChunks)
        {
            var instance = Instantiate(chunk, targetParent);
            instance.name = $"Course Chunk { i++ }";

            int lengthInWorldUnits = chunk.Length;
            float zOffset = accumulatedDistance + lengthInWorldUnits / 2f;
            instance.transform.position = origin + Vector3.forward * zOffset;

            // Squish on the z-axis very slightly. Prevent the back face of adjacent
            // meshes from rendering over eachother
            instance.transform.localScale = Vector3.one - (Vector3.forward * 0.0001f);

            accumulatedDistance += lengthInWorldUnits;

            chunks.Add(instance);
        }

        // Finish line and stuff
        foreach (var chunk in tailChunks)
        {
            int lengthInWorldUnits = chunk.Length;
            float zOffset = accumulatedDistance + lengthInWorldUnits / 2f;
            chunk.transform.position = origin + Vector3.forward * zOffset;
            accumulatedDistance += lengthInWorldUnits;

            chunks.Add(chunk);
        }

        tailChunks.Clear();
    }
}
