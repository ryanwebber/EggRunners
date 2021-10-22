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
        public CourseRunner instance;
        public Nullable<RunnerResult> result;

        public bool IsRunning => !result.HasValue;
    }

    [SerializeField]
    private GameState gameState;

    [SerializeField]
    private CourseProgress progressTracker;

    [SerializeField]
    private Transform SpawnPoint;

    private float runStartTime = 0f;
    private RunnerState[] runners;

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
                ResetRunnerAtStartPosition(runners[i].instance);

                runners[i].result = null;
                runners[i].instance.Events.OnRunnerDidReset?.Invoke();
            }
        };
    }

    private void LateUpdate()
    {
        if (runners == null)
            return;

        foreach (var runner in runners)
        {
            progressTracker.RecordProgress(runner.instance.transform.position);
        }
    }

    private void ResetRunnerAtStartPosition(CourseRunner runner)
    {
        Debug.Log("Resetting player at spawn", this);
        runner.transform.position = SpawnPoint.position;
        runner.transform.localScale = Vector3.one;
        runner.transform.up = Vector3.up;
        runner.MainInput.IsInputLocked = true;
    }    

    public void LoadRoster(CourseRoster roster)
    {
        Assert.AreEqual(null, runners);
        runners = new RunnerState[roster.players.Count];

        for (int i = 0; i < roster.players.Count; i++)
        {
            var contestant = roster.players[i];
            var instance = Instantiate(contestant.PlayerPrefab);
            var playerIndex = i;

            if (contestant.InputSource != null)
                instance.MakePlayableByHuman(contestant.InputSource);

            ResetRunnerAtStartPosition(instance);
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

                if (runners.All(state => !state.IsRunning))
                {
                    // Round complete
                    var results = new RunResult
                    {
                        roster = roster,
                        runnerResults = runners.Select(r => r.result.GetValueOrDefault()).ToArray(),
                    };

                    gameState.Events.OnAllRunnersFinished?.Invoke(results);
                }
            };

            runners[playerIndex] = new RunnerState
            {
                instance = instance,
                result = null,
            };
        }
    }
}
