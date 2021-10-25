using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ChunkService : MonoBehaviour
{
    [SerializeField]
    private GameState gameState;

    [SerializeField]
    private Vector3 origin;

    [SerializeField]
    private CourseChunk startingChunk;

    [SerializeField]
    private CourseChunk finishingChunk;

    [SerializeField]
    private CourseBuilder builder;

    private List<CourseChunk> chunks;

    private void Awake()
    {
        if (chunks == null)
            chunks = new List<CourseChunk>();

        gameState.OnRegisterServices += () => gameState.Services.RegisterService(this);
        gameState.Events.OnCountDownEnd += () =>
        {
            Debug.Log("Enabling dynamic obstacles...");
            foreach (var chunk in chunks)
                chunk.OnChunkDynamicsActivated?.Invoke();
        };

        gameState.Events.OnCourseShouldReset += () =>
        {
            Debug.Log("Resetting dynamic obstacles...");
            foreach (var chunk in chunks)
                chunk.OnChunkDynamicsReset?.Invoke();
        };
    }

    public void LoadChunkPrefabs(IEnumerable<CourseChunk> newChunks)
    {
        Assert.AreEqual(chunks.Count, 0);

        if (startingChunk != null)
            chunks.Add(startingChunk);

        foreach (var chunk in newChunks)
        {
            var instance = Instantiate(chunk);
            chunks.Add(instance);
        }

        if (finishingChunk != null)
            chunks.Add(finishingChunk);

        builder.LayoutCourse(chunks);
    }
}
