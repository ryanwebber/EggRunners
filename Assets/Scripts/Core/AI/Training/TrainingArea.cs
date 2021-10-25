using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;

public class TrainingArea : MonoBehaviour
{
    public Event OnCourseReset;
    public Event OnCourseActivate;
    public Event OnCourseCompleted;

    [SerializeField]
    private CourseBuilder builder;

    [SerializeField]
    private CourseChunk startChunk;

    [SerializeField]
    private CourseChunk finishChunk;

    [SerializeField]
    private Transform courseParent;

    [SerializeField]
    private TrainingCourse courseOverride;

    [SerializeField]
    private TrainingCourse[] seasons;

    public int CurrentLevel { get; set; } = -1;

    private List<CourseChunk> disposableChunks;

    private void Awake()
    {
        disposableChunks = new List<CourseChunk>();
        OnCourseReset += () => ResetCourse();
    }

    private void Start()
    {
        if (courseOverride != null)
        {
            var chunks = CreateChunkInstances(courseOverride.chunks);
            builder.LayoutCourse(chunks);
        }
    }

    private void ResetCourse()
    {
        if (courseOverride != null)
            return;

        float levelValue = Academy.Instance.EnvironmentParameters.GetWithDefault("level", 0f);
        int levelNumber = Mathf.Abs(Mathf.FloorToInt(levelValue) % seasons.Length);

        if (levelNumber != CurrentLevel || CurrentLevel < 0)
        {
            Debug.Log($"Level changed: {levelNumber}", this);
            CurrentLevel = levelNumber;

            foreach (var chunk in disposableChunks)
                Destroy(chunk.gameObject);

            disposableChunks.Clear();

            var chunks = CreateChunkInstances(seasons[levelNumber].chunks);
            builder.LayoutCourse(chunks);
        }
    }

    private IEnumerable<CourseChunk> CreateChunkInstances(IEnumerable<CourseChunk> prefabs)
    {
        if (startChunk != null)
            yield return startChunk;

        if (seasons.Length > 0)
        {
            foreach (var chunkPrefab in prefabs)
            {
                var instance = Instantiate(chunkPrefab, courseParent);
                disposableChunks.Add(instance);
                yield return instance;
            }
        }

        if (finishChunk != null)
            yield return finishChunk;
    }
}
