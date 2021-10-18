using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CourseHud : MonoBehaviour
{
    [SerializeField]
    private GameState gameState;

    [SerializeField]
    private TextReceiver countdownText;

    private void Awake()
    {
        gameState.Events.OnCountDownTick += state =>
        {
            var remaining = state.secondsRemaining;
            var total = state.countdownDuration;

            if (remaining > 0)
            {
                countdownText.UpdateText($"{remaining}", remaining != total);
            }
            else
            {
                countdownText.UpdateText("Go!");
                StartCoroutine(Coroutines.After(1f, () =>
                {
                    countdownText.UpdateText("");
                }));
            }
        };
    }
}
