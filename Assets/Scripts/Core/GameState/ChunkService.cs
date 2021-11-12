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

    public Vector3 GetPositionAlongCourseDolly(float zOffset)
    {
        if (chunks.Count == 0)
            return new Vector3(0f, 0f, zOffset);

        float yCurrent = chunks[0].Bounds.center.y;
        float zCurrent = chunks[0].Bounds.min.z;

        CourseChunk currentChunk = null;
        for (int i = 0; i < chunks.Count; i++)
        {
            if (chunks[i].Bounds.min.z <= zOffset && chunks[i].Bounds.max.z > zOffset)
            {
                currentChunk = chunks[i];
                break;
            }

            Debug.DrawLine(
                new Vector3(0f, yCurrent, zCurrent),
                new Vector3(0f, yCurrent + chunks[i].EndHeight, zCurrent + chunks[i].Length),
                Color.red);

            yCurrent += chunks[i].EndHeight;
            zCurrent += chunks[i].Length;
        }

        if (currentChunk == null)
            currentChunk = chunks[chunks.Count - 1];

        if (currentChunk.Length == 0)
            return new Vector3(0f, yCurrent, zOffset);

        float pctChunkComplete = (zOffset - zCurrent) / currentChunk.Length;
        float yLerped = Mathf.Lerp(yCurrent, yCurrent + currentChunk.EndHeight, pctChunkComplete);
        return new Vector3(0f, yLerped, zOffset);
    }
}
