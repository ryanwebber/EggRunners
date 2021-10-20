using UnityEngine;
using System.Collections;
using System.Linq;

public class TrialRunController : MonoBehaviour
{
    [SerializeField]
    private GameState gameState;

    public void StartRound(TrialParameters parameters)
    {
        gameState.BootGame();
        gameState.Services.GetService<PlayerService>().SpawnRunners(new PlayerRegistration[] { parameters.PlayerInfo });
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
