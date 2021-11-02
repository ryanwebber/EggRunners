using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerService : MonoBehaviour
{
    private struct RunnerState
    {
        public int index;
        public CourseRunner instance;
        public Nullable<RunnerResult> result;

        public bool IsRunning => !result.HasValue;
    }

    [SerializeField]
    private GameState gameState;

    [SerializeField]
    private CourseProgress progressTracker;

    [SerializeField]
    private Transform averagePlayerPosition;

    [SerializeField]
    private CourseFinishZone finishZone;

    [SerializeField]
    private Transform SpawnPoint;

    private float runStartTime = 0f;
    private RunnerState[] runners;
    private CourseRoster roster;

    private void Awake()
    {
        gameState.OnRegisterServices += () => gameState.Services.RegisterService(this);
        gameState.Events.OnCountDownEnd += () =>
        {
            runStartTime = Time.time;
            foreach (var runner in runners)
            {
                runner.instance.MainInput.IsInputLocked = false;
            }
        };

        gameState.Events.OnCourseShouldReset += () =>
        {
            Debug.Log("Resetting players");
            for (int i = 0; i < runners.Length; i++)
            {
                ResetRunner(runners[i].instance);

                runners[i].result = null;
                runners[i].instance.Events.OnRunnerDidReset?.Invoke();
            }

            RandomizeStartPositions();
        };
    }

    private void LateUpdate()
    {
        if (runners == null)
            return;

        Vector3 cumulativeOffset = Vector3.zero;

        foreach (var runner in runners)
        {
            var instance = runner.instance;
            var playerIndex = runner.index;

            if (runner.IsRunning)
            {
                progressTracker.RecordProgress(instance.Center);
                cumulativeOffset += instance.Center - progressTracker.transform.position;
            }

            if (finishZone.FinishBounds.Contains(instance.Center) && runners[playerIndex].IsRunning)
            {
                runners[playerIndex].result = new RunnerResult
                {
                    playerIndex = playerIndex,
                    runDuration = Time.time - runStartTime,
                    eliminationDetails = null,
                };

                instance.Events.OnRunnerFinishDetected?.Invoke();

                if (IsRoundComplete())
                    StartCoroutine(Coroutines.After(2f, () => BuildResultAndCompleteRound()));
            }
        }

        // Max the camera should move
        float maxXOffset = 6;

        // Elasticity of camera movement
        float dampeningFactor = 0.05f;

        // x:0 => y:0
        // x:inf => y:1
        float yScalar = 1 - 1 / (1 + dampeningFactor * Mathf.Abs(cumulativeOffset.x));

        // Final horizontal shift        
        averagePlayerPosition.localPosition = new Vector3(yScalar * maxXOffset * Mathf.Sign(cumulativeOffset.x), 0, 0);
    }

    private void ResetRunner(CourseRunner runner)
    {
        Debug.Log("Resetting player at spawn", this);
        runner.Center = SpawnPoint.position;
        runner.transform.localScale = Vector3.one;
        runner.transform.up = Vector3.up;
        runner.MainInput.IsInputLocked = true;
    }

    private void RandomizeStartPositions()
    {
        int runnerCount = runners.Length;
        float spacing = 1.8f;

        List<Transform> transforms = new List<Transform>(runners.Select(r => r.instance.transform));
        Collections.Shuffle(transforms);

        var xOffset = ((runnerCount - 1) * spacing) / 2f;
        for (int i = 0; i < runnerCount; i++)
        {
            transforms[i].position = SpawnPoint.position + (Vector3.left * xOffset) + (Vector3.right * i * spacing);
        }
    }

    public void LoadRoster(CourseRoster roster)
    {
        Assert.AreEqual(null, runners);
        this.runners = new RunnerState[roster.players.Count];
        this.roster = roster;

        for (int i = 0; i < roster.players.Count; i++)
        {
            var contestant = roster.players[i];
            var instance = Instantiate(contestant.PlayerPrefab);
            var playerIndex = i;

            if (contestant.InputSource != null)
                instance.MakePlayableByHuman(contestant.InputSource);

            ResetRunner(instance);
            instance.Events.OnRunnerDidSpawn?.Invoke();

            instance.Events.OnRunnerEliminationSequenceComplete += () =>
            {
                if (!runners[playerIndex].IsRunning)
                    return;

                runners[playerIndex].result = new RunnerResult
                {
                    playerIndex = playerIndex,
                    runDuration = Time.time - runStartTime,
                    eliminationDetails = new Elimination
                    {
                        // TODO: distance in course, player knockout
                    },
                };

                if (IsRoundComplete())
                    BuildResultAndCompleteRound();
            };

            runners[playerIndex] = new RunnerState
            {
                index = playerIndex,
                instance = instance,
                result = null,
            };
        }

        RandomizeStartPositions();
    }

    private bool IsRoundComplete() => runners.All(state => !state.IsRunning);

    private void BuildResultAndCompleteRound()
    {
        var results = new RunResult
        {
            roster = roster,
            runnerResults = runners.Select(r => r.result.GetValueOrDefault()).ToArray(),
        };

        gameState.Events.OnAllRunnersFinished?.Invoke(results);
    }
}
