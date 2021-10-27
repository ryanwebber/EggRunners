using UnityEngine;
using System.Collections;

public class FinishChunkDetector : MonoBehaviour
{
    public Event<CourseRunner> OnRunnerEnterFinishArea;

    [SerializeField]
    private LayerMask playerLayerMask;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayerMask) != 0 && other.TryGetComponent<CourseRunner>(out var runner))
            OnRunnerEnterFinishArea?.Invoke(runner);
    }
}
