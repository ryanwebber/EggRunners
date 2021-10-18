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
        var countdownIterator = CountDownState.WithDuration(3, () => new WaitForSeconds(1f));
        foreach (var tick in countdownIterator)
        {
            yield return tick.step;
            gameState.Events.OnCountDownTick?.Invoke(tick.state);
        }
    }
}
