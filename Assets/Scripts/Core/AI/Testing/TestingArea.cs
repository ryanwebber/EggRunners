using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestingArea : MonoBehaviour
{
    [SerializeField]
    private CourseRunner runner;

    [SerializeField]
    private FinishChunkDetector finishArea;

    [SerializeField]
    private CourseBuilder builder;

    [SerializeField]
    private CourseChunk startChunk;

    [SerializeField]
    private CourseChunk finishChunk;

    [SerializeField]
    private Transform courseParent;

    [SerializeField]
    private TrainingCourse[] courseList;

    public int CurrentLevel { get; set; } = -1;

    private List<CourseChunk> disposableChunks;
    private Vector3 playerSpawnPosition;

    private void Awake()
    {
        disposableChunks = new List<CourseChunk>();
        playerSpawnPosition = runner.transform.position;

        runner.Events.OnRunnerEliminationSequenceComplete += () => ResetCourse();
        runner.Events.OnRunnerFinishDetected += () => ResetCourse();

        finishArea.OnRunnerEnterFinishArea += runner => runner.Events.OnRunnerFinishDetected?.Invoke();
    }

    private void Start()
    {
        ResetCourse();
    }

    private void ResetCourse()
    {
        runner.transform.position = playerSpawnPosition;
        runner.transform.localScale = Vector3.one;
        runner.transform.rotation = Quaternion.identity;

        runner.Events.OnRunnerDidReset?.Invoke();
        runner.MainInput.IsInputLocked = false;

        if (courseList == null || courseList.Length == 0)
            return;

        int levelNumber = (CurrentLevel + 1) % courseList.Length;

        if (levelNumber != CurrentLevel)
        {
            Debug.Log($"Level changed: {levelNumber}", this);
            CurrentLevel = levelNumber;

            foreach (var chunk in disposableChunks)
                Destroy(chunk.gameObject);

            disposableChunks.Clear();

            var chunks = CreateChunkInstances(courseList[levelNumber].chunks);
            builder.LayoutCourse(chunks);
        }
    }

    private IEnumerable<CourseChunk> CreateChunkInstances(IEnumerable<CourseChunk> prefabs)
    {
        if (startChunk != null)
            yield return startChunk;

        foreach (var chunkPrefab in prefabs)
        {
            var instance = Instantiate(chunkPrefab, courseParent);
            disposableChunks.Add(instance);
            yield return instance;
        }

        if (finishChunk != null)
            yield return finishChunk;
    }

    private void OnValidate()
    {
        if (disposableChunks == null)
            disposableChunks = new List<CourseChunk>();

        void ClearChunks()
        {
            foreach (var chunk in disposableChunks)
                Destroy(chunk.gameObject);

            disposableChunks.Clear();
        }

        if (courseList.Length == 0 && disposableChunks.Count > 0)
            ClearChunks();

        if (courseList.Length > 0 && courseList[0].chunks.Length != disposableChunks.Count)
        {
            ClearChunks();
            var chunks = CreateChunkInstances(courseList[0].chunks);
            builder.LayoutCourse(chunks);
        }    
    }
}
