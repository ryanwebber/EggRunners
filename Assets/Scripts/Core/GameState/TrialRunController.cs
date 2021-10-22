using UnityEngine;
using System.Collections;
using System.Linq;

public class TrialRunController : MonoBehaviour
{
    [SerializeField]
    private GameState gameState;

    [SerializeField]
    private TrialUIController ui;

    public void BootGame(TrialParameters parameters)
    {
        gameState.BootGame();
        gameState.Services.GetService<ChunkService>().LoadChunkPrefabs(parameters.CourseLayout.ChunkList);
        gameState.Services.GetService<PlayerService>().LoadRoster(parameters.PlayerRoster);

        gameState.Events.OnAllRunnersFinished += _ =>
        {
            // TODO: Go through UI for this
            gameState.Events.OnCourseShouldReset?.Invoke();
            StartCoroutine(Coroutines.After(1f, () => StartCoroutine(BeginRound())));
        };

        StartCoroutine(BeginRound());
    }

    private IEnumerator BeginRound()
    {
        gameState.Events.OnCountDownBegin?.Invoke();
        var countdownIterator = CountDownState.WithCount(3, () => new WaitForSeconds(1f));
        foreach (var tick in countdownIterator)
        {
            yield return tick.step;
            gameState.Events.OnCountDownTick?.Invoke(tick.state);
        }


        yield return new WaitForSeconds(0.75f);
        gameState.Events.OnCountDownEnd?.Invoke();
    }
}
