using UnityEngine;
using System.Collections;

public class CourseRunnerLifecycle : MonoBehaviour
{
    private Event OnPlayerApparentElimination;

    [SerializeField]
    private LayerMask physicsBoundsLayer;

    [SerializeField]
    private float minimimHeightBeforeElimination;

    [SerializeField]
    private CMF.Mover movementController;

    [SerializeField]
    private CourseRunnerEvents events;

    private bool wasEliminated = false;

    private void Awake()
    {
        OnPlayerApparentElimination += () =>
        {
            Debug.Log("Player left the playable bounds. Possible elimination...", gameObject);

            // Get rid of the players velocity
            movementController.BaseLayerMask = 0;
            movementController.SetVelocity(Vector3.zero);

            wasEliminated = true;

            events.OnRunnerElimination?.Invoke();
        };
    }

    private void Update()
    {
        if (transform.position.y < minimimHeightBeforeElimination && !wasEliminated)
            OnPlayerApparentElimination?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & physicsBoundsLayer.value) != 0 && !wasEliminated)
        {
            OnPlayerApparentElimination?.Invoke();
        }
    }
}
