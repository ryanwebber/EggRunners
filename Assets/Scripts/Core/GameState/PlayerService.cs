using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerService : MonoBehaviour
{
    public Event<Elimination> OnRunnerEliminated;

    [SerializeField]
    private GameState gameState;

    [SerializeField]
    private Transform SpawnPoint;

    private List<CourseRunner> allRunners;
    public IReadOnlyList<CourseRunner> Runners => allRunners;

    private void Awake()
    {
        allRunners = new List<CourseRunner>();
        gameState.OnRegisterServices += () => gameState.Services.RegisterService(this);
    }

    public void SpawnRunners(IEnumerable<PlayerRegistration> players)
    {
        foreach (var contestant in players)
        {
            var instance = Instantiate(contestant.PlayerPrefab);
            instance.transform.position = SpawnPoint.position;
            instance.MainInput.Bind(contestant.InputSource);

            allRunners.Add(instance);
        }
    }
}
