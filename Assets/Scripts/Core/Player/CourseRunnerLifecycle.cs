using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CourseRunnerLifecycle : MonoBehaviour
{
    private Event OnPlayerApparentElimination;

    [SerializeField]
    private LayerMask physicsBoundsLayer;

    [SerializeField]
    private float minimimHeightBeforeElimination;

    [SerializeField]
    private CMFMovementController movementController;

    [SerializeField]
    private CourseRunnerEvents events;

    private bool wasEliminated = false;

    private void Awake()
    {
        OnPlayerApparentElimination += () =>
        {
            if (wasEliminated)
                return;

            wasEliminated = true;
            events.OnRunnerEliminationDetected?.Invoke();

            // Ensure they don't collide with the level anymore
            movementController.Mover.BaseLayerMask = 0;
            movementController.Mover.GetBodyCollider().enabled = false;
        };

        var initiaCollisionMask = movementController.Mover.BaseLayerMask;
        var oldGravity = movementController.gravity;
        events.OnRunnerDidReset += () =>
        {
            wasEliminated = false;
            movementController.gravity = oldGravity;
            movementController.Mover.BaseLayerMask = initiaCollisionMask;
            movementController.Mover.GetBodyCollider().enabled = true;
        };
    }

    private void Update()
    {
        if (transform.position.y < minimimHeightBeforeElimination && !wasEliminated)
        {
            Debug.Log("Player is below kill threshold. Will eliminate...", this);

            OnPlayerApparentElimination?.Invoke();

            // Halt forward/backward momentum
            var momentum = movementController.GetMomentum();
            momentum.z = 0f;
            movementController.SetMomentum(momentum);

            DOTween.Sequence()
                .AppendInterval(1f)
                .Append(transform.DOScale(0f, 0.4f))
                .OnComplete(() =>
                {
                    Debug.Log("Player elimination sequence complete", this);
                    movementController.gravity = 0f;
                    events.OnRunnerEliminationSequenceComplete?.Invoke();
                })
                .Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & physicsBoundsLayer.value) != 0 && !wasEliminated)
        {
            Debug.Log("Player has left main physics boundary. Will Eliminate...", this);

            OnPlayerApparentElimination?.Invoke();

            // Keep track of what the gravity used to be
            var oldGravity = movementController.gravity;

            // Freeze the player
            movementController.gravity = 0;

            // Reset momentum next fixed update cycle. This is closely
            // tied to the movement controller logic which will manipulate
            // momentum if grounding is lost (which it is when we turn off
            // gravity) so we have to wait one cycle
            StartCoroutine(Coroutines.OnFixedUpdate(2, () => movementController.SetMomentum(Vector3.zero)));

            DOTween.Sequence()
                .AppendInterval(1f)
                .AppendCallback(() =>
                {
                    movementController.gravity = oldGravity * 1.5f;
                })
                .AppendInterval(1f)
                .Append(transform.DOScale(0f, 0.4f))
                .OnComplete(() =>
                {
                    Debug.Log("Player elimination sequence complete", this);
                    //movementController.gravity = 0f;
                    events.OnRunnerEliminationSequenceComplete?.Invoke();
                })
                .Play();
        }
    }
}
